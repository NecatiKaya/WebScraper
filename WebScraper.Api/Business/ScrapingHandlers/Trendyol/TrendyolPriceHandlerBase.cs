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

    protected decimal? ParseTrendyolPriceSpan(string? span)
    {
        if (span is null)
        {
            return null;
        }

        string onlyPriceText = span.Replace(" ", "").ToLower().Replace("tl", "").Trim(',', '.', '"');

        if (decimal.TryParse(onlyPriceText, out decimal price))
        {
            return price;
        }

        return null;
    }
}