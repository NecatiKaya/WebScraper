using HtmlAgilityPack;
using WebScraper.Api.Business.ScrapingHandlers.Amazon.PriceHandlers;
using WebScraper.Api.Business.ScrapingHandlers.Trendyol;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Business.Parsers
{
    internal class TrendyolParser : IHtmlParser
    {
        public string Html { get; private set; }

        public TrendyolParser(string html)
        {
            if (html is null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            Html = html;
        }

        public ProductPriceInformation? Parse()
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.OptionCheckSyntax = false;
            htmlDoc.LoadHtml(Html);

            TrendyolPriceHandlerBase singleHandler = new TrendyolSinglePriceHandler();
            TrendyolPriceHandlerBase multiPriceHandler = new TrendyolMultiPriceHandler();
            TrendyolPriceHandlerBase noPriceHandler = new TrendyolNoPriceHandler();
            singleHandler.SetNextHandler(multiPriceHandler);
            multiPriceHandler.SetNextHandler(noPriceHandler);
            ProductPriceInformation? priceInformation = singleHandler.HandleRequst(htmlDoc);
            return priceInformation;
        }
    }
}
