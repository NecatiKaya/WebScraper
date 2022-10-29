using WebScraper.Api.Data.Models;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Utilities;

public static class MappingHelper
{
    public static ScraperVisit GetScraperVisit(Product product, ProductPriceInformation? trendyolPriceInformation, ProductPriceInformation? amazonPriceInformation)
    {
        ScraperVisit visit = new ScraperVisit()
        {
            ProductId = product.Id,
            VisitDate = DateTime.Now,
            Notified = false
        };
        if (trendyolPriceInformation is not null)
        {
            visit.TrendyolCurrentDiscountAsAmount = trendyolPriceInformation.CurrentDiscountAsAmount;
            visit.TrendyolCurrentDiscountAsPercentage = trendyolPriceInformation.CurrentDiscountAsPercentage;
            visit.TrendyolCurrentPrice = trendyolPriceInformation.CurrentPrice;
            visit.TrendyolPreviousPrice = trendyolPriceInformation.PreviousPrice;
        }
        if (amazonPriceInformation is not null)
        {
            visit.AmazonCurrentDiscountAsAmount = amazonPriceInformation.CurrentDiscountAsAmount;
            visit.AmazonCurrentDiscountAsPercentage = amazonPriceInformation.CurrentDiscountAsPercentage;
            visit.AmazonCurrentPrice = amazonPriceInformation.CurrentPrice;
            visit.AmazonPreviousPrice = amazonPriceInformation.PreviousPrice;
        }

        if (trendyolPriceInformation is not null && amazonPriceInformation is not null)
        {
            bool needToNotify = false;

            decimal calculatedDifferenceWithAmount = trendyolPriceInformation.CurrentPrice - amazonPriceInformation.CurrentPrice;
            decimal calculatedDifferenceWithPercentage = calculatedDifferenceWithAmount * 100 / trendyolPriceInformation.CurrentPrice;

            if (product.RequestedPriceDifferenceWithAmount is not null)
            {
                needToNotify = calculatedDifferenceWithAmount >= product.RequestedPriceDifferenceWithAmount;
            }

            if (product.RequestedPriceDifferenceWithPercentage is not null)
            {
                needToNotify = calculatedDifferenceWithPercentage >= product.RequestedPriceDifferenceWithPercentage;
            }

            visit.CalculatedPriceDifferenceAsPercentage = calculatedDifferenceWithPercentage;
            visit.CalculatedPriceDifferenceAsAmount = calculatedDifferenceWithAmount;
            visit.RequestedPriceDifferenceAsPercentage = product!.RequestedPriceDifferenceWithPercentage;
            visit.RequestedPriceDifferenceAsAmount = product!.RequestedPriceDifferenceWithAmount;
            visit.NeedToNotify = needToNotify;
        }

        return visit;
    }
}