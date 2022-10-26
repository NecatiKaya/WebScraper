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

        public static async Task<HtmlInfo?> GetHtml(string url, Websites websites)
        {
            if (url is null)
            {
                return null;
            }

            string? html = null;
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage response = await client.SendAsync(request);
                using (StreamReader reader = new(await response.Content.ReadAsStreamAsync()))
                {
                    html = await reader.ReadToEndAsync();
                }
            }

            return new HtmlInfo(websites, html);
        }
    }
}
