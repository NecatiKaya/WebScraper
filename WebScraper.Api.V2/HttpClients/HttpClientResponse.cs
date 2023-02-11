using System.Net;

namespace WebScraper.Api.V2.HttpClients;

public readonly struct HttpClientResponse
{
    public HttpClientResponse(HttpStatusCode? statusCode, string? statusText, string? html, string? requestId, IReadOnlyDictionary<string, string>? requestHeaders, IReadOnlyDictionary<string, string>? responseHeaders, 
HttpClientCookie[]? cookies, bool isBotDetected = false)
    {
        Status = statusCode;
        StatusText = statusText;
        ContentHtml = html;
        ResponseHeaders = responseHeaders;
        RequestHeaders = requestHeaders;
        Cookies = cookies;
        RequestId = requestId;
        IsBotDetected = isBotDetected;
    }

    public string? ContentHtml { get; }

    public HttpStatusCode? Status { get; }

    public string? StatusText { get; }

    public HttpClientCookie[]? Cookies { get; }

    public IReadOnlyDictionary<string, string>? RequestHeaders { get; }

    public IReadOnlyDictionary<string, string>? ResponseHeaders { get; }

    public string? RequestId { get; }

    public bool IsBotDetected { get; }
}