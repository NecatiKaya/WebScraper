using HtmlAgilityPack;
using WebScraper.Api.V2.Business.Parsing.Trendyol;
using WebScraper.Api.V2.Data.Dto;

namespace WebScraper.Api.V2.Business.Parsing;

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