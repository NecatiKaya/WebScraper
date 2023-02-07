using HtmlAgilityPack;
using WebScraper.Api.V2.Data.Dto;

namespace WebScraper.Api.V2.Business.Parsing.Trendyol;

public class TrendyolMultiPriceHandler : TrendyolPriceHandlerBase
{
    public override ProductPriceInformation? HandleRequst(HtmlDocument doc)
    {
        HtmlNode? multiPriceContainer = doc.DocumentNode.SelectNodes("//div[@class='product-price-container']")?.FirstOrDefault();
        if (multiPriceContainer != null && multiPriceContainer.HasChildNodes)
        {
            HtmlNode? priceInfoNode = multiPriceContainer.Descendants("div")?.Last();
            if (priceInfoNode is not null)
            {
                if (priceInfoNode.ChildNodes.Count == 2)
                {
                    decimal? previousPrice = ParseTrendyolPriceSpan(priceInfoNode.ChildNodes[0].InnerText);
                    decimal? currentPrice = ParseTrendyolPriceSpan(priceInfoNode.ChildNodes[1].InnerText);

                    ProductPriceInformation priceInformation = new ProductPriceInformation();
                    if (previousPrice is not null)
                    {
                        priceInformation.PreviousPrice = previousPrice.Value;
                    }

                    if (currentPrice is not null)
                    {
                        priceInformation.CurrentPrice = currentPrice.Value;
                    }

                    priceInformation.CurrentDiscountAsAmount = priceInformation.PreviousPrice - priceInformation.CurrentPrice;
                    priceInformation.CurrentDiscountAsPercentage = priceInformation.CurrentDiscountAsAmount * 100 / priceInformation.PreviousPrice;
                    return priceInformation;
                }
            }
        }

        return Successor?.HandleRequst(doc);
    }
}