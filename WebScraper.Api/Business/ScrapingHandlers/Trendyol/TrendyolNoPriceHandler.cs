using HtmlAgilityPack;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Business.ScrapingHandlers.Trendyol;
public class TrendyolNoPriceHandler : TrendyolPriceHandlerBase
{
    public override ProductPriceInformation? HandleRequst(HtmlDocument doc)
    {
        return null;
    }
}