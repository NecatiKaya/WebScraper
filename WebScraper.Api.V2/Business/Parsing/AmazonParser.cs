using HtmlAgilityPack;
using WebScraper.Api.V2.Business.Parsing.Amazon.PriceHandlers;
using WebScraper.Api.V2.Data.Dto;

namespace WebScraper.Api.V2.Business.Parsing;

internal class AmazonParser : IHtmlParser
{
    public string Html { get; private set; }

    public AmazonParser(string html)
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

        AmazonPriceHandlerBase noPriceHandler = new AmazonNoPriceHandler();
        AmazonPriceHandlerBase singlePriceHandler = new AmazonSinglePriceHandler();
        AmazonPriceHandlerBase multiPriceHandler = new AmazonMultiplePriceHandler();
        singlePriceHandler.SetNextHandler(multiPriceHandler);
        multiPriceHandler.SetNextHandler(noPriceHandler);

        ProductPriceInformation? priceInformation = singlePriceHandler.HandleRequst(htmlDoc);
        return priceInformation;
    }
}