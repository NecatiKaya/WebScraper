using HtmlAgilityPack;
using WebScraper.Api.V2.Data.Dto;

namespace WebScraper.Api.V2.Business.Parsing.Amazon.PriceHandlers;

public abstract class AmazonPriceHandlerBase
{
    protected AmazonPriceHandlerBase? Successor;

    public AmazonPriceHandlerBase()
    {

    }

    public void SetNextHandler(AmazonPriceHandlerBase nextHandler)
    {
        Successor = nextHandler;
    }

    public abstract ProductPriceInformation? HandleRequst(HtmlDocument doc);
}