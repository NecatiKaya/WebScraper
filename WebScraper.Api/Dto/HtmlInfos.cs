namespace WebScraper.Api.Dto
{
    public class HtmlInfos
    {
        public HtmlInfo? TrendyolHtmlInfo { get; set; }

        public HtmlInfo? AmazonHtmlInfo { get; set; }

        public Data.Models.Product Product { get; set; }
    }
}
