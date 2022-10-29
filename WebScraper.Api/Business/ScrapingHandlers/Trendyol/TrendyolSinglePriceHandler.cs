using HtmlAgilityPack;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Business.ScrapingHandlers.Trendyol;

public class TrendyolSinglePriceHandler : TrendyolPriceHandlerBase
{
    public override ProductPriceInformation? HandleRequst(HtmlDocument doc)
    {
        HtmlNode? onePriceContainer = doc.DocumentNode.SelectNodes("//div[@class='product-price-container']")?.FirstOrDefault();
        if (onePriceContainer != null && onePriceContainer.HasChildNodes)
        {
            //HtmlNode? singlePrice = onePriceContainer.SelectNodes("./span[@class='prc-dsc']")?.First();
            HtmlNode? singlePrice = onePriceContainer.Descendants("span")?.Last();
            if (singlePrice != null)
            {
                decimal? currentPrice = ParseTrendyolPriceSpan(singlePrice.InnerText);
                if (currentPrice is not null)
                {
                    ProductPriceInformation priceInformation = new ProductPriceInformation()
                    {
                        CurrentDiscountAsAmount = 0,
                        CurrentDiscountAsPercentage = 0,
                        CurrentPrice = currentPrice.Value,
                        PreviousPrice = currentPrice.Value
                    };

                    return priceInformation;
                }
            }
        }

        return Successor?.HandleRequst(doc);
    }
}