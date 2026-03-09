namespace Prolog.NET.Model;

/// <summary>Declares the Prolog name of a relation type.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class PrologRelationNameAttribute(string Name) : Attribute
{
    public string Name { get; } = Name;
}

/// <summary>Base class for arity-conveying relation attributes.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public abstract class PrologRelationAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class PrologRelationAttribute<T1> : PrologRelationAttribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class PrologRelationAttribute<T1, T2> : PrologRelationAttribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class PrologRelationAttribute<T1, T2, T3> : PrologRelationAttribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class PrologRelationAttribute<T1, T2, T3, T4> : PrologRelationAttribute { }

/// <summary>Base class for functor attributes; carries the Prolog functor name.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public abstract class PrologFunctorAttribute(string Name) : Attribute
{
    public string Name { get; } = Name;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class PrologFunctorAttribute<T1>(string name) : PrologFunctorAttribute(name) { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class PrologFunctorAttribute<T1, T2>(string name) : PrologFunctorAttribute(name) { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class PrologFunctorAttribute<T1, T2, T3>(string name) : PrologFunctorAttribute(name) { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class PrologFunctorAttribute<T1, T2, T3, T4>(string name) : PrologFunctorAttribute(name) { }
