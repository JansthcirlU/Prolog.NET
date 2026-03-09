using Microsoft.Extensions.DependencyInjection;

namespace Prolog.NET.Swipl;

/// <summary>
/// Extension methods for registering <see cref="PrologEngine"/> with
/// Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class PrologEngineExtensions
{
    /// <summary>
    /// Registers the <see cref="PrologEngine"/> as a singleton service and initializes
    /// the SWI-Prolog runtime when first resolved.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="args">
    /// Optional command-line arguments passed to SWI-Prolog (e.g. <c>["--quiet"]</c>).
    /// </param>
    public static IServiceCollection AddPrologEngine(
        this IServiceCollection services,
        string[]? args = null)
        => services.AddSingleton(_ => PrologEngine.Initialize(args));
}
