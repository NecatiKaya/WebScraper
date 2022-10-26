using HtmlAgilityPack;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Business.ScrapingHandlers.Amazon.PriceHandlers;

public class AmazonSinglePriceHandler : AmazonPriceHandlerBase
{
    public override ProductPriceInformation? HandleRequst(HtmlDocument doc)
    {
        HtmlNode? onePriceContainer = doc.GetElementbyId("corePriceDisplay_desktop_feature_div");
        if (onePriceContainer != null && onePriceContainer.HasChildNodes)
        {
            HtmlNode? wholePartNode = onePriceContainer.SelectNodes("//span[@class='a-price-whole']").FirstOrDefault();
            if (wholePartNode != null)
            {
                string? wholePart = wholePartNode.InnerText?.Trim(',', '.', '"');
                string? fractionPart = wholePartNode.NextSibling?.InnerText?.Trim(',', '.', '"');

                if (decimal.TryParse(wholePart + "," + fractionPart, out decimal price))
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