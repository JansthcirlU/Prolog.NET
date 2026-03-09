namespace Prolog.NET.Model;

/// <summary>
/// Wildcard type for attribute type parameters. Used as a placeholder
/// to represent "any argument" in relation/functor attribute declarations.
/// Never instantiated.
/// </summary>
public sealed class PrologArgument
{
    private PrologArgument() { }
}

/// <summary>Marker interface for Prolog functor types.</summary>
public interface IFunctor { }

/// <summary>Marker interface for Prolog relation types.</summary>
public interface IRelation { }
