using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Prolog.NET.Model.Generator;

internal sealed record RelationModel(
    string TypeName,
    string Namespace,
    string PrologName,
    IReadOnlyList<int> Arities);

internal sealed record FunctorModel(
    string TypeName,
    string Namespace,
    string PrologName,
    int Arity);

[Generator]
public sealed class SchemaGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor NotPartialDescriptor = new(
        id: "PNET001",
        title: "Type must be declared partial",
        messageFormat: "Type '{0}' must be declared partial to use schema attributes",
        category: "Prolog.NET.Model.Generator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // ── Relation pipeline ─────────────────────────────────────────────────
        IncrementalValuesProvider<RelationModel> relations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Prolog.NET.Model.PrologRelationNameAttribute",
                predicate: static (node, _) =>
                    node is RecordDeclarationSyntax or ClassDeclarationSyntax,
                transform: static (ctx, _) => ExtractRelationModel(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        context.RegisterSourceOutput(relations,
            static (ctx, model) => EmitRelation(ctx, model));

        // ── Functor pipeline ──────────────────────────────────────────────────
        IncrementalValuesProvider<FunctorModel> functors = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => HasPrologFunctorAttribute(node),
                transform: static (ctx, _) => ExtractFunctorModel(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        context.RegisterSourceOutput(functors,
            static (ctx, model) => EmitFunctor(ctx, model));
    }

    // ── Extraction ────────────────────────────────────────────────────────────

    private static RelationModel? ExtractRelationModel(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return null;
        }

        if (!IsPartial(symbol, ctx))
        {
            return null;
        }

        // Prolog name from PrologRelationNameAttribute
        string? prologName = null;
        foreach (AttributeData attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "PrologRelationNameAttribute"
                && attr.ConstructorArguments.Length == 1)
            {
                prologName = attr.ConstructorArguments[0].Value as string;
                break;
            }
        }

        if (prologName is null)
        {
            return null;
        }

        // Collect arities from all PrologRelationAttribute<...> instances
        List<int> arities = [];
        foreach (AttributeData attr in symbol.GetAttributes())
        {
            INamedTypeSymbol? attrClass = attr.AttributeClass;
            if (attrClass is null)
            {
                continue;
            }

            if (IsPrologRelationAttribute(attrClass))
            {
                int arity = attrClass.TypeArguments.Length;
                if (!arities.Contains(arity))
                {
                    arities.Add(arity);
                }
            }
        }

        string ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? ""
            : symbol.ContainingNamespace.ToDisplayString();

        return new RelationModel(symbol.Name, ns, prologName, arities);
    }

    private static bool HasPrologFunctorAttribute(SyntaxNode node)
    {
        AttributeListSyntax[]? attrLists = node switch
        {
            RecordDeclarationSyntax r => System.Linq.Enumerable.ToArray(r.AttributeLists),
            ClassDeclarationSyntax c => System.Linq.Enumerable.ToArray(c.AttributeLists),
            StructDeclarationSyntax s => System.Linq.Enumerable.ToArray(s.AttributeLists),
            _ => null,
        };

        if (attrLists is null)
        {
            return false;
        }

        foreach (AttributeListSyntax list in attrLists)
        {
            foreach (AttributeSyntax attr in list.Attributes)
            {
                string name = attr.Name.ToString();
                // Strip generic type args from name for comparison
                string simpleName = name.Contains('<') ? name.Substring(0, name.IndexOf('<')) : name;
                // Strip leading namespace qualifiers
                int lastDot = simpleName.LastIndexOf('.');
                if (lastDot >= 0)
                {
                    simpleName = simpleName.Substring(lastDot + 1);
                }

                if (simpleName == "PrologFunctor" || simpleName == "PrologFunctorAttribute")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static FunctorModel? ExtractFunctorModel(GeneratorSyntaxContext ctx)
    {
        if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        if (!IsPartialFromSyntax(ctx.Node))
        {
            ctx.SemanticModel.Compilation.GetDiagnostics(); // no-op; diagnostics go via ctx below
            // We can't emit diagnostics from CreateSyntaxProvider transform easily; skip for now
            return null;
        }

        // Find the first PrologFunctorAttribute<...>
        INamedTypeSymbol? attrClass = null;
        AttributeData? functorAttr = null;
        foreach (AttributeData attr in symbol.GetAttributes())
        {
            INamedTypeSymbol? cls = attr.AttributeClass;
            if (cls is null)
            {
                continue;
            }

            if (cls.Name.StartsWith("PrologFunctorAttribute") && cls.IsGenericType)
            {
                attrClass = cls;
                functorAttr = attr;
                break;
            }
        }

        if (functorAttr is null || attrClass is null)
        {
            return null;
        }

        if (functorAttr.ConstructorArguments.Length == 0)
        {
            return null;
        }

        string? prologName = functorAttr.ConstructorArguments[0].Value as string;
        if (prologName is null)
        {
            return null;
        }

        int arity = attrClass.TypeArguments.Length;

        string ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? ""
            : symbol.ContainingNamespace.ToDisplayString();

        return new FunctorModel(symbol.Name, ns, prologName, arity);
    }

    private static bool IsPrologRelationAttribute(INamedTypeSymbol attrClass)
    {
        // Matches PrologRelationAttribute, PrologRelationAttribute<T1>, etc.
        string name = attrClass.Name;
        if (name != "PrologRelationAttribute")
        {
            return false;
        }

        // Check it's in Prolog.NET.Model namespace
        string? ns = attrClass.ContainingNamespace?.ToDisplayString();
        return ns == "Prolog.NET.Model";
    }

    private static bool IsPartial(INamedTypeSymbol symbol, GeneratorAttributeSyntaxContext ctx)
    {
        foreach (SyntaxReference syntaxRef in symbol.DeclaringSyntaxReferences)
        {
            SyntaxNode syntax = syntaxRef.GetSyntax();
            if (IsPartialFromSyntax(syntax))
            {
                return true;
            }
        }

        ctx.SemanticModel.Compilation.GetDiagnostics(); // force compilation (no-op)
        // Report diagnostic via production context is not available here; emit in RegisterSourceOutput
        return false;
    }

    private static bool IsPartialFromSyntax(SyntaxNode node)
    {
        SyntaxTokenList modifiers = node switch
        {
            RecordDeclarationSyntax r => r.Modifiers,
            ClassDeclarationSyntax c => c.Modifiers,
            StructDeclarationSyntax s => s.Modifiers,
            _ => default,
        };
        return modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    // ── Emission ──────────────────────────────────────────────────────────────

    private static void EmitRelation(SourceProductionContext ctx, RelationModel model)
    {
        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(model.Namespace))
        {
            sb.AppendLine($"namespace {model.Namespace};");
            sb.AppendLine();
        }

        sb.AppendLine($"public partial record {model.TypeName}");
        sb.AppendLine("{");

        foreach (int arity in model.Arities)
        {
            string paramList = string.Join(", ",
                Enumerable.Range(0, arity).Select(i => $"global::Prolog.NET.Model.PrologTerm arg{i}"));
            string argList = string.Join(", ",
                Enumerable.Range(0, arity).Select(i => $"arg{i}"));

            string arrayLiteral = arity == 0 ? "[]" : $"[{argList}]";

            sb.AppendLine();
            sb.AppendLine($"    public static global::Prolog.NET.Model.PrologFact Fact({paramList})");
            sb.AppendLine($"        => new(\"{model.PrologName}\", {arrayLiteral});");

            sb.AppendLine();
            sb.AppendLine($"    public static global::Prolog.NET.Model.BodyGoal Query({paramList})");
            sb.AppendLine($"        => new global::Prolog.NET.Model.Call(\"{model.PrologName}\", {arrayLiteral});");
        }

        sb.AppendLine();
        sb.AppendLine($"    public static global::Prolog.NET.Model.RuleBuilder Rule()");
        sb.AppendLine($"        => new(\"{model.PrologName}\");");

        sb.AppendLine("}");

        ctx.AddSource($"{model.TypeName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void EmitFunctor(SourceProductionContext ctx, FunctorModel model)
    {
        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(model.Namespace))
        {
            sb.AppendLine($"namespace {model.Namespace};");
            sb.AppendLine();
        }

        sb.AppendLine($"public partial record {model.TypeName}");
        sb.AppendLine("{");

        string paramList = string.Join(", ",
            Enumerable.Range(0, model.Arity).Select(i => $"global::Prolog.NET.Model.PrologTerm arg{i}"));
        string argList = string.Join(", ",
            Enumerable.Range(0, model.Arity).Select(i => $"arg{i}"));

        string arrayLiteral = model.Arity == 0 ? "[]" : $"[{argList}]";

        sb.AppendLine();
        sb.AppendLine($"    public static global::Prolog.NET.Model.PrologTerm Of({paramList})");
        sb.AppendLine($"        => new global::Prolog.NET.Model.PrologCompound(\"{model.PrologName}\", {arrayLiteral});");

        sb.AppendLine("}");

        ctx.AddSource($"{model.TypeName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
