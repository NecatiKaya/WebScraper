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
            HtmlNode? wholePartNode = onePriceContainer.SelectNodes("//span[@class='prc-dsc']").FirstOrDefault();
            if (wholePartNode != null)
            {
                string? wholePart = wholePartNode.InnerText?.Replace(" ", "").ToLower().Replace("tl", "").Trim(',', '.', '"');

                if (decimal.TryParse(wholePart, out decimal price))
                {
                    ProductPriceInformation priceInformation = new ProductPriceInformation()
                    {
                        CurrentDiscountAsAmount = 0,
                        CurrentDiscountAsPercentage = 0,
                        CurrentPrice = price,
                        PreviousPrice = price
                    };

                    return priceInformation;
                }
            }
        }

        return Successor?.HandleRequst(doc);
    }
}