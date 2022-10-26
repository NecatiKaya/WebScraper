using HtmlAgilityPack;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Business.ScrapingHandlers.Amazon.PriceHandlers;

public class AmazonMultiplePriceHandler : AmazonPriceHandlerBase
{
    public override ProductPriceInformation? HandleRequst(HtmlDocument doc)
    {
        HtmlNode? multiplePriceContainer = doc.GetElementbyId("corePrice_desktop");
        if (multiplePriceContainer != null && multiplePriceContainer.HasChildNodes)
        {
            string? previousPrice = multiplePriceContainer.SelectNodes("//tr[1]/td[2]/span/span")?.FirstOrDefault()?.InnerText?.ToLowerInvariant().Replace("tl", "");
            string? currentPrice = multiplePriceContainer.SelectNodes("//tr[2]/td[2]/span/span")?.FirstOrDefault()?.InnerText?.ToLowerInvariant().Replace("tl", "");
            //string? discountAmount = multiplePriceContainer.SelectNodes("//tr[3]/td[2]/span/span/span[1]").FirstOrDefault()?.InnerText?.ToLowerInvariant().Replace("tl", "");
            if (previousPrice != null && currentPrice != null && decimal.TryParse(previousPrice, out decimal _previousPrice)
                && decimal.TryParse(currentPrice, out decimal _currentPrice)
                //&& decimal.TryParse(discountAmount, out decimal _discountAmount)
                )
            {
                decimal _discountAmount = _previousPrice - _currentPrice;
                decimal discountRatio = Math.Round(_discountAmount * (decimal)100.0 / _previousPrice, 3);
                ProductPriceInformation priceInformation = new ProductPriceInformation()
                {
                    CurrentDiscountAsAmount = _discountAmount,
                    CurrentDiscountAsPercentage = discountRatio,
                    CurrentPrice = _currentPrice,
                    PreviousPrice = _previousPrice
                };

                return priceInformation;
            }
        }

        return Successor?.HandleRequst(doc);
    }
}