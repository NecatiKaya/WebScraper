using WebScraper.Api.Data.Models;

namespace WebScraper.Api.Dto
{
    public class HtmlInfo
    {
        public Websites Website { get; set; }

        public string Html { get; set; }

        public HtmlInfo(Websites websites, string html)
        {
            Html = html;
            Website = websites;
        }

        public static async Task<HtmlInfo?> GetHtmlInfo(string url, Websites websites)
        {
            if (url is null)
            {
                return null;
            }

            string? html = null;
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage response = await client.SendAsync(request);
                using (StreamReader reader = new(await response.Content.ReadAsStreamAsync()))
                {
                    html = await reader.ReadToEndAsync();
                }
            }
            finally
            {
                 
            }

            return new HtmlInfo(websites, html);
        }

        private static HttpClient _httpClient = new HttpClient();
        public static async Task<string?> GetHtml(string url)
        {
            if (url is null)
            {
                return null;
            }

            string? html = null;
            try
            {
                 
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                using (HttpResponseMessage clonedResponse = await CloneResponseAsync(response))
                {
                    using (StreamReader reader = new(await clonedResponse.Content.ReadAsStreamAsync()))
                    {
                        html = await reader.ReadToEndAsync();
                    }
                }
            }
            finally
            {

            }

            return html;
        }

        private static async Task<HttpResponseMessage> CloneResponseAsync(HttpResponseMessage response)
        {
            HttpResponseMessage newResponse = new HttpResponseMessage(response.StatusCode);
            MemoryStream ms = new MemoryStream();

            foreach (var header in response.Headers)
            {
                newResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            if (response.Content != null)
            {
                await response.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                newResponse.Content = new StreamContent(ms);

                foreach (var header in response.Content.Headers)
                {
                    newResponse.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

            }
            return newResponse;
        }
    }
}
