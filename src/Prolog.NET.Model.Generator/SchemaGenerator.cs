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
    IReadOnlyList<int> Arities,
    string? Module = null,
    string? ContainingTypeName = null,
    bool ContainingTypeIsStatic = false);

internal sealed record FunctorModel(
    string TypeName,
    string Namespace,
    string PrologName,
    int Arity,
    string? Module = null,
    string? ContainingTypeName = null,
    bool ContainingTypeIsStatic = false);

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

        string? moduleName = null;
        foreach (AttributeData attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "PrologModuleAttribute"
                && attr.ConstructorArguments.Length == 1)
            {
                moduleName = attr.ConstructorArguments[0].Value as string;
                break;
            }
        }

        string? containingTypeName = null;
        bool containingTypeIsStatic = false;
        if (symbol.ContainingType is { } ct)
        {
            containingTypeName = ct.Name;
            containingTypeIsStatic = ct.IsStatic;
        }

        return new RelationModel(symbol.Name, ns, prologName, arities, moduleName, containingTypeName, containingTypeIsStatic);
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

        string? moduleName = null;
        foreach (AttributeData attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "PrologModuleAttribute"
                && attr.ConstructorArguments.Length == 1)
            {
                moduleName = attr.ConstructorArguments[0].Value as string;
                break;
            }
        }

        string? containingTypeName = null;
        bool containingTypeIsStatic = false;
        if (symbol.ContainingType is { } ct)
        {
            containingTypeName = ct.Name;
            containingTypeIsStatic = ct.IsStatic;
        }

        return new FunctorModel(symbol.Name, ns, prologName, arity, moduleName, containingTypeName, containingTypeIsStatic);
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

        bool nested = model.ContainingTypeName is not null;
        string i = nested ? "    " : "";
        string ii = i + "    ";
        string iii = ii + "    ";

        if (nested)
        {
            string staticMod = model.ContainingTypeIsStatic ? "static " : "";
            sb.AppendLine($"public {staticMod}partial class {model.ContainingTypeName}");
            sb.AppendLine("{");
        }

        sb.AppendLine($"{i}public partial record {model.TypeName}");
        sb.AppendLine($"{i}{{");

        foreach (int arity in model.Arities)
        {
            string paramList = string.Join(", ",
                Enumerable.Range(0, arity).Select(idx => $"global::Prolog.NET.Model.PrologTerm arg{idx}"));
            string argList = string.Join(", ",
                Enumerable.Range(0, arity).Select(idx => $"arg{idx}"));

            string arrayLiteral = arity == 0 ? "[]" : $"[{argList}]";

            string moduleArg = model.Module is not null ? $", \"{model.Module}\"" : "";
            sb.AppendLine();
            sb.AppendLine($"{ii}public static global::Prolog.NET.Model.PrologFact Fact({paramList})");
            sb.AppendLine($"{iii}=> new(\"{model.PrologName}\", {arrayLiteral}{moduleArg});");

            sb.AppendLine();
            sb.AppendLine($"{ii}public static global::Prolog.NET.Model.BodyGoal Query({paramList})");
            sb.AppendLine($"{iii}=> new global::Prolog.NET.Model.Call(\"{model.PrologName}\", {arrayLiteral}{moduleArg});");
        }

        sb.AppendLine();
        sb.AppendLine($"{ii}public static global::Prolog.NET.Model.RuleBuilder Rule()");
        sb.AppendLine($"{iii}=> new(\"{model.PrologName}\");");

        sb.AppendLine();
        sb.AppendLine($"{ii}public static global::Prolog.NET.Model.PrologDatabaseItem Multifile()");
        string declList = string.Join(", ", model.Arities.Select(a =>
            $"new global::Prolog.NET.Model.PrologMultifileDeclaration(\"{model.PrologName}\", {a})"));
        sb.AppendLine($"{iii}=> global::Prolog.NET.Model.PrologDatabaseItem.FromEntries([{declList}]);");

        sb.AppendLine($"{i}}}");

        if (nested)
        {
            sb.AppendLine("}");
        }

        string hintName = nested ? $"{model.ContainingTypeName}_{model.TypeName}.g.cs" : $"{model.TypeName}.g.cs";
        ctx.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
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

        bool nested = model.ContainingTypeName is not null;
        string i = nested ? "    " : "";
        string ii = i + "    ";
        string iii = ii + "    ";

        if (nested)
        {
            string staticMod = model.ContainingTypeIsStatic ? "static " : "";
            sb.AppendLine($"public {staticMod}partial class {model.ContainingTypeName}");
            sb.AppendLine("{");
        }

        sb.AppendLine($"{i}public partial record {model.TypeName}");
        sb.AppendLine($"{i}{{");

        string paramList = string.Join(", ",
            Enumerable.Range(0, model.Arity).Select(idx => $"global::Prolog.NET.Model.PrologTerm arg{idx}"));
        string argList = string.Join(", ",
            Enumerable.Range(0, model.Arity).Select(idx => $"arg{idx}"));

        string arrayLiteral = model.Arity == 0 ? "[]" : $"[{argList}]";

        sb.AppendLine();
        sb.AppendLine($"{ii}public static global::Prolog.NET.Model.PrologTerm Of({paramList})");
        sb.AppendLine($"{iii}=> new global::Prolog.NET.Model.PrologCompound(\"{model.PrologName}\", {arrayLiteral});");

        sb.AppendLine($"{i}}}");

        if (nested)
        {
            sb.AppendLine("}");
        }

        string hintName = nested ? $"{model.ContainingTypeName}_{model.TypeName}_Functor.g.cs" : $"{model.TypeName}_Functor.g.cs";
        ctx.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
