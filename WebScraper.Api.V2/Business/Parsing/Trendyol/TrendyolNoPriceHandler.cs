using HtmlAgilityPack;
using WebScraper.Api.V2.Data.Dto;

namespace WebScraper.Api.V2.Business.Parsing.Trendyol;
public class TrendyolNoPriceHandler : TrendyolPriceHandlerBase
{
    public override ProductPriceInformation? HandleRequst(HtmlDocument doc)
    {
        return null;
    }
}