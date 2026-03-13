namespace Prolog.NET.Documentation.Conceptual.Server.Rest;

internal abstract record RestResponse
{
    internal sealed record OkResponse<T>(T Body) : RestResponse where T : RestPrologBody;
    internal sealed record BadRequestResponse<T>(T Body) : RestResponse where T : RestPrologBody;
    internal sealed record ProblemResponse<T>(T Body) : RestResponse where T : RestPrologBody;
    internal sealed record NotImplementedResponse<T>(T Body) : RestResponse where T : RestPrologBody;

    internal static OkResponse<T> Ok<T>(T body) where T : RestPrologBody
        => new(body);

    internal static BadRequestResponse<T> BadRequest<T>(T body) where T : RestPrologBody
        => new(body);

    internal static ProblemResponse<T> Problem<T>(T body) where T : RestPrologBody
        => new(body);

    internal static NotImplementedResponse<T> NotImplemented<T>(T body) where T : RestPrologBody
        => new(body);
}
