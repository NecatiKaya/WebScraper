using Flurl.Http;
using System.Net.Http.Headers;

namespace WebScraper.Api.V2.HttpClients.Flurl;

public static class FlurlClientResponseExtensions
{
    public static IReadOnlyDictionary<string, string> GetHeadersAsDictionary<T>(this T? headers) where T : HttpHeaders
    {
        if (headers == null || headers.Count() == 0)
        {
            return new Dictionary<string, string>().AsReadOnly();
        }

        Dictionary<string, string> result = new Dictionary<string, string>();
        foreach (KeyValuePair<string, IEnumerable<string>> eachItem in headers)
        {
            result.Add(eachItem.Key, eachItem.Value?.First() ?? string.Empty);
        }

        return result.AsReadOnly();
    }

    public static IReadOnlyDictionary<string, string> GetHeadersAsDictionary(this IFlurlResponse response, bool responseHeaders = true)
    {
        if (responseHeaders)
        {
            return response.ResponseMessage.Headers?.GetHeadersAsDictionary() ?? new Dictionary<string, string>().AsReadOnly();
        }

        return response.ResponseMessage.RequestMessage?.Headers?.GetHeadersAsDictionary() ?? new Dictionary<string, string>().AsReadOnly();
    }
}