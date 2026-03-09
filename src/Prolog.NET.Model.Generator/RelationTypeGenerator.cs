using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Prolog.NET.Model.Generator;

[Generator]
public sealed class RelationTypeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find field/property declarations adorned with [GenerateRelationTypes]
        var targets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => HasGenerateRelationTypesAttribute(node),
                transform: static (ctx, _) => ExtractDatabaseFromNode(ctx))
            .Where(static r => r is not null)
            .Select(static (r, _) => r!);

        // Collect ALL annotated databases and emit a single file so the hint name
        // never collides when multiple [GenerateRelationTypes] fields are present.
        var allRelations = targets
            .Collect()
            .Select(static (collections, _) =>
            {
                var merged = new List<RelationModel>();
                foreach (var coll in collections)
                    merged.AddRange(coll);
                return (IReadOnlyList<RelationModel>)merged;
            });

        context.RegisterSourceOutput(allRelations, static (ctx, relations) => Emit(ctx, relations));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Extraction
    // ──────────────────────────────────────────────────────────────────────────

    private static bool HasGenerateRelationTypesAttribute(SyntaxNode node)
    {
        AttributeListSyntax[]? attrLists = node switch
        {
            FieldDeclarationSyntax f => System.Linq.Enumerable.ToArray(f.AttributeLists),
            PropertyDeclarationSyntax p => System.Linq.Enumerable.ToArray(p.AttributeLists),
            _ => null,
        };
        if (attrLists is null) return false;
        foreach (var list in attrLists)
            foreach (var attr in list.Attributes)
            {
                var name = attr.Name.ToString();
                if (name == "GenerateRelationTypes" || name == "GenerateRelationTypesAttribute"
                    || name.EndsWith(".GenerateRelationTypes")
                    || name.EndsWith(".GenerateRelationTypesAttribute"))
                    return true;
            }
        return false;
    }

    private static IReadOnlyList<RelationModel>? ExtractDatabaseFromNode(
        GeneratorSyntaxContext ctx)
    {
        // Locate the initializer expression (ObjectCreationExpression for `new PrologDatabase(…)`)
        ExpressionSyntax? initExpr = ctx.Node switch
        {
            FieldDeclarationSyntax f =>
                f.Declaration.Variables.FirstOrDefault()?.Initializer?.Value,
            PropertyDeclarationSyntax p =>
                p.Initializer?.Value ?? p.ExpressionBody?.Expression,
            _ => null,
        };

        if (initExpr is null)
            return null;

        // Unwrap `new PrologDatabase([…])` or `new PrologDatabase(new[] {…})` etc.
        CollectionExpressionSyntax? collection = null;

        if (initExpr is ObjectCreationExpressionSyntax oce)
        {
            var args = oce.ArgumentList?.Arguments;
            if (args?.Count == 1)
            {
                var arg = args.Value[0].Expression;
                collection = arg as CollectionExpressionSyntax;
            }
        }
        else if (initExpr is ImplicitObjectCreationExpressionSyntax ioc)
        {
            var args = ioc.ArgumentList?.Arguments;
            if (args?.Count == 1)
            {
                var arg = args.Value[0].Expression;
                collection = arg as CollectionExpressionSyntax;
            }
        }

        if (collection is null)
            return null;

        var clauses = new List<ClauseModel>();

        foreach (var element in collection.Elements)
        {
            if (element is not ExpressionElementSyntax expr)
                continue;

            var clause = TryParseClause(expr.Expression);
            if (clause is not null)
                clauses.Add(clause);
        }

        if (clauses.Count == 0)
            return null;

        // Group by functor
        return clauses
            .GroupBy(c => c.Functor)
            .Select(g => new RelationModel(g.Key, g.ToList()))
            .ToList();
    }

    private static ClauseModel? TryParseClause(ExpressionSyntax expr)
    {
        if (expr is not InvocationExpressionSyntax inv)
            return null;

        var memberName = GetMemberName(inv.Expression);

        if (memberName is "PrologFact.Of" or "PrologFact")
        {
            return TryParseFact(inv.ArgumentList.Arguments);
        }

        if (memberName is "PrologRule.Of")
        {
            return TryParseRuleOf(inv.ArgumentList.Arguments);
        }

        if (memberName is "PrologRule.Create")
        {
            return TryParseRuleCreate(inv.ArgumentList.Arguments);
        }

        return null;
    }

    private static ClauseModel? TryParseFact(SeparatedSyntaxList<ArgumentSyntax> args)
    {
        if (args.Count == 0)
            return null;

        var functor = ExtractStringLiteral(args[0].Expression);
        if (functor is null)
            return null;

        var headArgs = new List<HeadArg>();
        for (int i = 1; i < args.Count; i++)
        {
            var ha = ParseHeadArg(args[i].Expression);
            if (ha is null)
                return null;
            headArgs.Add(ha);
        }

        return new ClauseModel(functor, headArgs, System.Array.Empty<BodyGoal>());
    }

    private static ClauseModel? TryParseRuleOf(SeparatedSyntaxList<ArgumentSyntax> args)
    {
        // PrologRule.Of(functor, headArgsList, bodyList)
        if (args.Count != 3)
            return null;

        var functor = ExtractStringLiteral(args[0].Expression);
        if (functor is null)
            return null;

        var headArgs = ParseTermList(args[1].Expression, ParseHeadArg);
        var body = ParseTermList(args[2].Expression, ParseBodyGoal);

        if (headArgs is null || body is null)
            return null;

        return new ClauseModel(functor, headArgs, body);
    }

    private static ClauseModel? TryParseRuleCreate(SeparatedSyntaxList<ArgumentSyntax> args)
    {
        // PrologRule.Create(functor, varName, build: v => (headArgs, body))
        if (args.Count != 3)
            return null;

        var functor = ExtractStringLiteral(args[0].Expression);
        var varName = ExtractStringLiteral(args[1].Expression);
        if (functor is null || varName is null)
            return null;

        // The third arg should be a lambda: v => (headArgs, bodyGoals)
        var lambda = args[2].Expression as SimpleLambdaExpressionSyntax;
        if (lambda is null)
            return null;

        // Identify the lambda parameter name so we can recognise its uses
        var paramName = lambda.Parameter.Identifier.Text;

        // Lambda body should be a tuple expression: (headArgs, body)
        TupleExpressionSyntax? tuple = null;
        if (lambda.Body is TupleExpressionSyntax t)
            tuple = t;
        else if (lambda.Body is ParenthesizedExpressionSyntax p
                 && p.Expression is TupleExpressionSyntax pt)
            tuple = pt;

        if (tuple is null || tuple.Arguments.Count != 2)
            return null;

        var headArgs = ParseTermList(tuple.Arguments[0].Expression,
            e => ParseHeadArgWithVar(e, paramName, varName));
        var body = ParseTermList(tuple.Arguments[1].Expression,
            e => ParseBodyGoalWithVar(e, paramName, varName));

        if (headArgs is null || body is null)
            return null;

        return new ClauseModel(functor, headArgs, body);
    }

    // ── Term parsers ──────────────────────────────────────────────────────────

    private static HeadArg? ParseHeadArg(ExpressionSyntax expr)
        => ParseHeadArgWithVar(expr, null, null);

    private static HeadArg? ParseHeadArgWithVar(
        ExpressionSyntax expr, string? lambdaParam, string? varName)
    {
        // `new PrologInteger(0)` or `PrologInteger.Of(0)`
        if (TryParseIntegerTerm(expr, out var intVal))
            return new HeadArg.IntLiteral(intVal);

        // `new PrologAtom("foo")` or `PrologAtom.Of("foo")`
        if (TryParseAtomTerm(expr, out var atomVal))
            return new HeadArg.AtomLiteral(atomVal!);

        // `new PrologVariable("N")` or `PrologVariable.Of("N")` or the lambda param itself
        if (TryParseVariableTerm(expr, lambdaParam, varName, out var vn))
            return new HeadArg.Variable(vn!);

        // `PrologCompoundTerm.Of("s", …)` or `new PrologCompoundTerm("s", new[]{…})`
        if (TryParseCompoundTerm(expr, lambdaParam, varName, out var cFunctor, out var cArgs))
            return new HeadArg.Compound(cFunctor!, cArgs!);

        return null;
    }

    private static BodyGoal? ParseBodyGoal(ExpressionSyntax expr)
        => ParseBodyGoalWithVar(expr, null, null);

    private static BodyGoal? ParseBodyGoalWithVar(
        ExpressionSyntax expr, string? lambdaParam, string? varName)
    {
        // We only care about compound body goals that call a known functor
        if (TryParseCompoundTerm(expr, lambdaParam, varName, out var functor, out var args))
        {
            var varNames = args!
                .OfType<HeadArg.Variable>()
                .Select(v => v.Name)
                .ToList();
            return new BodyGoal.Call(functor!, varNames);
        }

        return new BodyGoal.Opaque();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool TryParseIntegerTerm(ExpressionSyntax expr, out long value)
    {
        value = 0;
        // new PrologInteger(0)
        if (expr is ObjectCreationExpressionSyntax oce
            && GetSimpleName(oce.Type) is "PrologInteger"
            && oce.ArgumentList?.Arguments.Count == 1)
        {
            return TryParseLongLiteral(oce.ArgumentList.Arguments[0].Expression, out value);
        }
        // PrologInteger.Of(0)
        if (expr is InvocationExpressionSyntax inv
            && GetMemberName(inv.Expression) is "PrologInteger.Of"
            && inv.ArgumentList.Arguments.Count == 1)
        {
            return TryParseLongLiteral(inv.ArgumentList.Arguments[0].Expression, out value);
        }
        return false;
    }

    private static bool TryParseAtomTerm(ExpressionSyntax expr, out string? name)
    {
        name = null;
        if (expr is ObjectCreationExpressionSyntax oce
            && GetSimpleName(oce.Type) is "PrologAtom"
            && oce.ArgumentList?.Arguments.Count == 1)
        {
            name = ExtractStringLiteral(oce.ArgumentList.Arguments[0].Expression);
            return name is not null;
        }
        if (expr is InvocationExpressionSyntax inv
            && GetMemberName(inv.Expression) is "PrologAtom.Of"
            && inv.ArgumentList.Arguments.Count == 1)
        {
            name = ExtractStringLiteral(inv.ArgumentList.Arguments[0].Expression);
            return name is not null;
        }
        return false;
    }

    private static bool TryParseVariableTerm(
        ExpressionSyntax expr,
        string? lambdaParam, string? varName,
        out string? resultName)
    {
        resultName = null;

        // The lambda parameter itself used directly, e.g. `v` in `v => (…, v, …)`
        if (lambdaParam is not null && varName is not null
            && expr is IdentifierNameSyntax id && id.Identifier.Text == lambdaParam)
        {
            resultName = varName;
            return true;
        }

        // new PrologVariable(new VariableName("N"))  or  new PrologVariable("N") (if constructor changes)
        if (expr is ObjectCreationExpressionSyntax oce
            && GetSimpleName(oce.Type) is "PrologVariable"
            && oce.ArgumentList?.Arguments.Count == 1)
        {
            resultName = ExtractVariableNameArg(oce.ArgumentList.Arguments[0].Expression);
            return resultName is not null;
        }

        // PrologVariable.Of("N")
        if (expr is InvocationExpressionSyntax inv
            && GetMemberName(inv.Expression) is "PrologVariable.Of"
            && inv.ArgumentList.Arguments.Count == 1)
        {
            resultName = ExtractStringLiteral(inv.ArgumentList.Arguments[0].Expression);
            return resultName is not null;
        }

        return false;
    }

    private static bool TryParseCompoundTerm(
        ExpressionSyntax expr,
        string? lambdaParam, string? varName,
        out string? functor,
        out IReadOnlyList<HeadArg>? args)
    {
        functor = null;
        args = null;

        // PrologCompoundTerm.Of("s", arg1, arg2, …)
        if (expr is InvocationExpressionSyntax inv
            && GetMemberName(inv.Expression) is "PrologCompoundTerm.Of"
            && inv.ArgumentList.Arguments.Count >= 2)
        {
            functor = ExtractStringLiteral(inv.ArgumentList.Arguments[0].Expression);
            if (functor is null) return false;

            var list = new List<HeadArg>();
            for (int i = 1; i < inv.ArgumentList.Arguments.Count; i++)
            {
                var a = ParseHeadArgWithVar(inv.ArgumentList.Arguments[i].Expression,
                                            lambdaParam, varName);
                if (a is null) return false;
                list.Add(a);
            }
            args = list;
            return true;
        }

        // new PrologCompoundTerm(new AtomName("s"), new[] { … })
        if (expr is ObjectCreationExpressionSyntax oce
            && GetSimpleName(oce.Type) is "PrologCompoundTerm"
            && oce.ArgumentList?.Arguments.Count == 2)
        {
            functor = ExtractAtomNameArg(oce.ArgumentList.Arguments[0].Expression);
            if (functor is null) return false;

            args = ParseTermList(oce.ArgumentList.Arguments[1].Expression,
                e => ParseHeadArgWithVar(e, lambdaParam, varName));
            return args is not null;
        }

        return false;
    }

    // Parses a collection/array expression and maps each element through a transform.
    private static IReadOnlyList<T>? ParseTermList<T>(
        ExpressionSyntax expr,
        System.Func<ExpressionSyntax, T?> parse)
        where T : class
    {
        IEnumerable<ExpressionSyntax>? elements = null;

        if (expr is CollectionExpressionSyntax ce)
        {
            elements = ce.Elements
                .OfType<ExpressionElementSyntax>()
                .Select(e => e.Expression);
        }
        else if (expr is ArrayCreationExpressionSyntax ace)
        {
            elements = ace.Initializer?.Expressions;
        }
        else if (expr is ImplicitArrayCreationExpressionSyntax iac)
        {
            elements = iac.Initializer.Expressions;
        }

        if (elements is null)
            return null;

        var result = new List<T>();
        foreach (var e in elements)
        {
            var item = parse(e);
            if (item is null) return null;
            result.Add(item);
        }
        return result;
    }

    private static string? ExtractVariableNameArg(ExpressionSyntax expr)
    {
        // new VariableName("N")
        if (expr is ObjectCreationExpressionSyntax oce
            && GetSimpleName(oce.Type) is "VariableName"
            && oce.ArgumentList?.Arguments.Count == 1)
            return ExtractStringLiteral(oce.ArgumentList.Arguments[0].Expression);
        // Could also be a plain string in future overloads
        return ExtractStringLiteral(expr);
    }

    private static string? ExtractAtomNameArg(ExpressionSyntax expr)
    {
        if (expr is ObjectCreationExpressionSyntax oce
            && GetSimpleName(oce.Type) is "AtomName"
            && oce.ArgumentList?.Arguments.Count == 1)
            return ExtractStringLiteral(oce.ArgumentList.Arguments[0].Expression);
        return ExtractStringLiteral(expr);
    }

    private static string? ExtractStringLiteral(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax lit
            && lit.IsKind(SyntaxKind.StringLiteralExpression))
            return lit.Token.ValueText;
        return null;
    }

    private static bool TryParseLongLiteral(ExpressionSyntax expr, out long value)
    {
        value = 0;
        if (expr is LiteralExpressionSyntax lit
            && lit.IsKind(SyntaxKind.NumericLiteralExpression)
            && lit.Token.Value is long or int)
        {
            value = System.Convert.ToInt64(lit.Token.Value);
            return true;
        }
        return false;
    }

    private static string? GetMemberName(ExpressionSyntax expr)
    {
        if (expr is MemberAccessExpressionSyntax ma)
            return $"{GetSimpleName(ma.Expression)}.{ma.Name.Identifier.Text}";
        return null;
    }

    private static string? GetSimpleName(ExpressionSyntax expr)
    {
        return expr switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            _ => null,
        };
    }

    private static string? GetSimpleName(TypeSyntax type)
    {
        return type switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            QualifiedNameSyntax qn => qn.Right.Identifier.Text,
            _ => null,
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Emission
    // ──────────────────────────────────────────────────────────────────────────

    private static void Emit(
        SourceProductionContext ctx,
        IReadOnlyList<RelationModel> relations)
    {
        // Build set of all known functor names so we can resolve cross-relation constraints
        var knownFunctors = new HashSet<string>(
            relations.Select(r => r.Functor),
            System.StringComparer.Ordinal);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Prolog.NET.Model.Generated;");
        sb.AppendLine();

        // Relations that are queried with variables in body goals across all clauses
        var bodyQueriedRelations = new HashSet<string>(System.StringComparer.Ordinal);
        foreach (var rel in relations)
            foreach (var clause in rel.Clauses)
                foreach (var goal in clause.Body)
                    if (goal is BodyGoal.Call call
                        && knownFunctors.Contains(call.Functor)
                        && call.ArgVariableNames.Count > 0)
                        bodyQueriedRelations.Add(call.Functor);

        foreach (var relation in relations)
        {
            EmitRelation(sb, relation, knownFunctors, bodyQueriedRelations, ctx);
        }

        ctx.AddSource("RelationTypes.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void EmitRelation(
        StringBuilder sb,
        RelationModel relation,
        HashSet<string> knownFunctors,
        HashSet<string> bodyQueriedRelations,
        SourceProductionContext ctx)
    {
        var className = Capitalise(relation.Functor);
        var ifaceName = $"I{className}";

        sb.AppendLine($"public static class {className}");
        sb.AppendLine("{");
        sb.AppendLine($"    public interface {ifaceName} {{ }}");
        sb.AppendLine();

        // Track emitted case names to detect duplicates
        var emittedCases = new HashSet<string>();

        foreach (var clause in relation.Clauses)
        {
            var caseInfo = DeriveCaseType(clause, knownFunctors);
            if (caseInfo is null)
                continue;

            var (caseName, typeParams, constraints, ctorParams) = caseInfo;

            if (!emittedCases.Add(caseName))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "PNET001",
                        "Duplicate case type",
                        $"Relation '{relation.Functor}' has duplicate case type name '{caseName}'. " +
                        "Merge the clauses or rename one.",
                        "Prolog.NET.Model.Generator",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None));
                continue;
            }

            EmitCaseStruct(sb, caseName, typeParams, constraints, ctorParams, ifaceName);
        }

        if (bodyQueriedRelations.Contains(relation.Functor))
        {
            sb.AppendLine($"    public readonly record struct Any<T>(T Value) : {ifaceName};");
        }

        // Query type
        sb.AppendLine();
        sb.AppendLine($"    public sealed record Query<T{className}>(");
        sb.AppendLine($"        global::Prolog.NET.Model.QueryArgument<T{className}> Arg)");
        sb.AppendLine($"        where T{className} : struct, {ifaceName};");

        // Factory methods
        foreach (var clause in relation.Clauses)
        {
            var caseInfo = DeriveCaseType(clause, knownFunctors);
            if (caseInfo is null)
                continue;
            var (caseName, typeParams, constraints, _) = caseInfo;

            if (typeParams.Count == 0)
            {
                sb.AppendLine();
                sb.AppendLine($"    public static Query<{caseName}> Query{caseName}(");
                sb.AppendLine($"        global::Prolog.NET.Model.QueryArgument<{caseName}> arg)");
                sb.AppendLine($"        => new(arg);");
            }
            else
            {
                var typeParamList = string.Join(", ", typeParams);
                var constraintClauses = string.Join(" ", constraints.Select(c => $"where {c}"));
                sb.AppendLine();
                sb.AppendLine($"    public static Query<{caseName}<{typeParamList}>> Query{caseName}<{typeParamList}>(");
                sb.AppendLine($"        global::Prolog.NET.Model.QueryArgument<{caseName}<{typeParamList}>> arg)");
                if (constraints.Count > 0)
                    sb.AppendLine($"        {constraintClauses}");
                sb.AppendLine($"        => new(arg);");
            }
        }

        if (bodyQueriedRelations.Contains(relation.Functor))
        {
            sb.AppendLine();
            sb.AppendLine($"    public static Query<Any<T>> QueryAny<T>(");
            sb.AppendLine($"        global::Prolog.NET.Model.QueryArgument<Any<T>> arg)");
            sb.AppendLine($"        => new(arg);");
        }

        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static void EmitCaseStruct(
        StringBuilder sb,
        string caseName,
        IReadOnlyList<string> typeParams,
        IReadOnlyList<string> constraints,
        IReadOnlyList<(string paramType, string paramName)> ctorParams,
        string ifaceName)
    {
        if (typeParams.Count == 0)
        {
            // Zero-argument fact → parameterless struct
            sb.AppendLine($"    public readonly record struct {caseName} : {ifaceName};");
        }
        else
        {
            var typeParamList = string.Join(", ", typeParams);
            var paramList = string.Join(", ", ctorParams.Select(p => $"{p.paramType} {p.paramName}"));
            sb.Append($"    public readonly record struct {caseName}<{typeParamList}>({paramList}) : {ifaceName}");
            sb.AppendLine();
            foreach (var c in constraints)
                sb.AppendLine($"        where {c}");
            sb.AppendLine("    ;");
        }
    }

    // ── Case type derivation ──────────────────────────────────────────────────

    private record CaseTypeInfo(
        string CaseName,
        IReadOnlyList<string> TypeParams,
        IReadOnlyList<string> Constraints,
        IReadOnlyList<(string paramType, string paramName)> CtorParams);

    private static CaseTypeInfo? DeriveCaseType(
        ClauseModel clause,
        HashSet<string> knownFunctors)
    {
        if (clause.HeadArgs.Count == 0)
        {
            // Atom-only fact — no case args, name = Capitalise(functor)
            return new CaseTypeInfo(
                Capitalise(clause.Functor),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<(string, string)>());
        }

        if (clause.HeadArgs.Count == 1)
        {
            return DeriveFromSingleArg(clause.HeadArgs[0], clause.Body, knownFunctors);
        }

        // Multi-arg: not yet supported — emit opaque name
        return null;
    }

    private static CaseTypeInfo? DeriveFromSingleArg(
        HeadArg arg,
        IReadOnlyList<BodyGoal> body,
        HashSet<string> knownFunctors)
    {
        switch (arg)
        {
            case HeadArg.IntLiteral intLit:
            {
                // e.g. even(0) → case name "Zero"
                var name = IntegerToName(intLit.Value);
                return new CaseTypeInfo(name,
                    System.Array.Empty<string>(),
                    System.Array.Empty<string>(),
                    System.Array.Empty<(string, string)>());
            }

            case HeadArg.AtomLiteral atom:
            {
                var name = Capitalise(atom.Name);
                return new CaseTypeInfo(name,
                    System.Array.Empty<string>(),
                    System.Array.Empty<string>(),
                    System.Array.Empty<(string, string)>());
            }

            case HeadArg.Variable var:
            {
                // Free variable in head — look up body constraints
                var constraint = ResolveVariableConstraint(var.Name, body, knownFunctors,
                    out bool ambiguous);

                // Unconstrained variable → type param T, no constraint beyond `struct`
                var typeParam = $"T{Capitalise(var.Name)}";
                var constraintStr = constraint is not null
                    ? $"{typeParam} : struct, global::Prolog.NET.Model.Generated.{Capitalise(constraint)}.I{Capitalise(constraint)}"
                    : $"{typeParam} : struct";

                if (ambiguous)
                {
                    // Emit a warning but still generate with `where T : struct`
                    // (warning is a comment in the generated file; ideally a diagnostic)
                }

                return new CaseTypeInfo(
                    Capitalise(var.Name),
                    new[] { typeParam },
                    new[] { constraintStr },
                    new[] { (typeParam, Capitalise(var.Name)) });
            }

            case HeadArg.Compound compound:
            {
                // e.g. even(s(N)) → case "S<TOdd>" with constraint from body
                var caseName = Capitalise(compound.Functor);

                var typeParams = new List<string>();
                var constraints = new List<string>();
                var ctorParams = new List<(string, string)>();

                int varIndex = 0;
                foreach (var inner in compound.Args)
                {
                    if (inner is HeadArg.Variable v)
                    {
                        var constraint = ResolveVariableConstraint(v.Name, body,
                            knownFunctors, out bool ambiguous);
                        var tp = $"T{Capitalise(v.Name)}";
                        typeParams.Add(tp);
                        var c = constraint is not null
                            ? $"{tp} : struct, global::Prolog.NET.Model.Generated.{Capitalise(constraint)}.I{Capitalise(constraint)}"
                            : $"{tp} : struct";
                        constraints.Add(c);
                        ctorParams.Add((tp, Capitalise(v.Name)));
                        varIndex++;
                    }
                    // Non-variable inner args are part of the struct shape — skip for now
                }

                return new CaseTypeInfo(caseName, typeParams, constraints, ctorParams);
            }
        }

        return null;
    }

    /// <summary>
    /// Looks up which body goals reference <paramref name="varName"/> and, if exactly
    /// one known functor is found, returns its name. Sets <paramref name="ambiguous"/>
    /// if multiple distinct known functors are found.
    /// </summary>
    private static string? ResolveVariableConstraint(
        string varName,
        IReadOnlyList<BodyGoal> body,
        HashSet<string> knownFunctors,
        out bool ambiguous)
    {
        ambiguous = false;
        var matched = new HashSet<string>();

        foreach (var goal in body)
        {
            if (goal is BodyGoal.Call call
                && knownFunctors.Contains(call.Functor)
                && call.ArgVariableNames.Contains(varName))
            {
                matched.Add(call.Functor);
            }
        }

        if (matched.Count == 1)
            return matched.First();

        if (matched.Count > 1)
        {
            ambiguous = true;
        }

        return null;
    }

    // ── String utilities ──────────────────────────────────────────────────────

    private static string Capitalise(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s.Substring(1);
    }

    private static string IntegerToName(long value)
    {
        return value switch
        {
            0 => "Zero",
            1 => "One",
            2 => "Two",
            3 => "Three",
            4 => "Four",
            5 => "Five",
            6 => "Six",
            7 => "Seven",
            8 => "Eight",
            9 => "Nine",
            _ => $"Int{(value < 0 ? "Neg" : "")}{System.Math.Abs(value)}",
        };
    }
}
