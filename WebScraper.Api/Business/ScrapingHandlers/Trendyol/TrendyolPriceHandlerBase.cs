using HtmlAgilityPack;
using WebScraper.Api.Dto;
namespace WebScraper.Api.Business.ScrapingHandlers.Trendyol;

public abstract class TrendyolPriceHandlerBase
{
    protected TrendyolPriceHandlerBase? Successor;

    public TrendyolPriceHandlerBase()
    {

    }

    public void SetNextHandler(TrendyolPriceHandlerBase nextHandler)
    {
        Successor = nextHandler;
    }

    public abstract ProductPriceInformation? HandleRequst(HtmlDocument doc);
}