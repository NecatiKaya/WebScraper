namespace WebScraper.Api.V2.HttpClients;

public static class HttpClientResponseExtensions
{
    public static bool HasSuccessFullStatusCode(this HttpClientResponse? response)
    {
        if (response is null || !response.HasValue || response.Value.Status is null)
        {
            return false;
        }

        return (int)response.Value.Status.Value >= 200 && (int)response.Value.Status.Value <= 299;
    }
}
