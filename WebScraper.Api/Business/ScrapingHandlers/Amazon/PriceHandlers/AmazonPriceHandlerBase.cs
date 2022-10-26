using HtmlAgilityPack;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Business.ScrapingHandlers.Amazon.PriceHandlers;

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