using System.Net;

namespace Songlink.Client.Exceptions;

public class SongLinkException : Exception
{
    public SongLinkException(string method,
        string url,
        HttpStatusCode statusCode,
        string? reasonPhrase,
        string? jsonContent)
        : base($"Songlink API {method} {url} response status code {statusCode} does not indicate success. Reason: {reasonPhrase ?? "UNKNOWN"}. Content {jsonContent ?? "NO CONTENT"}")
    { }
}
