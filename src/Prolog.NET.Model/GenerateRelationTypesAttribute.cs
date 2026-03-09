namespace Prolog.NET.Model;

/// <summary>
/// Apply to a field or property of type <see cref="PrologDatabase"/> to trigger
/// the Roslyn source generator that emits a typed relation hierarchy for every
/// functor defined in the database.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class GenerateRelationTypesAttribute : Attribute;
