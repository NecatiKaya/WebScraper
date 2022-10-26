﻿namespace WebScraper.Api.Dto.ScraperVisit
{
    public class AddScraperVisitDto
    {
        public int ProductId { get; set; }

        public DateTime VisitDate { get; set; }

        public ProductPriceInformation? AmazonPrice { get; set; }

        public ProductPriceInformation? TrendyolPrice { get; set; }

        public decimal? CalculatedPriceDiffrenceAsAmount { get; set; }

        public decimal? CalculatedPriceDiffrenceAsPercentage { get; set; }

        public decimal? RequestedPriceDifferenceAsAmount { get; set; }

        public decimal? RequestedPriceDifferenceAsPercentage { get; set; }

        public bool NeedToNotify { get; set; } = false;

        public bool Notified { get; set; } = false;

        public Data.Models.ScraperVisit ToScraperVisit()
        {
            Data.Models.ScraperVisit visit = new Data.Models.ScraperVisit()
            {
                ProductId = ProductId,
                AmazonCurrentDiscountAsPercentage = AmazonPrice?.CurrentDiscountAsPercentage,
                AmazonCurrentDiscountAsAmount = AmazonPrice?.CurrentDiscountAsAmount,
                AmazonCurrentPrice = AmazonPrice?.CurrentPrice,
                AmazonPreviousPrice = AmazonPrice?.PreviousPrice,
                TrendyolCurrentDiscountAsPercentage = TrendyolPrice?.CurrentDiscountAsPercentage,
                TrendyolCurrentDiscountAsAmount = TrendyolPrice?.CurrentDiscountAsAmount,
                TrendyolCurrentPrice = TrendyolPrice?.CurrentPrice,
                TrendyolPreviousPrice = TrendyolPrice?.PreviousPrice,
                CalculatedPriceDifferenceAsAmount = CalculatedPriceDiffrenceAsAmount,
                CalculatedPriceDifferenceAsPercentage = CalculatedPriceDiffrenceAsPercentage,
                RequestedPriceDifferenceAsAmount = RequestedPriceDifferenceAsAmount,
                RequestedPriceDifferenceAsPercentage = RequestedPriceDifferenceAsPercentage,
                NeedToNotify = NeedToNotify,
                Notified = Notified,
                VisitDate = VisitDate
            };
            return visit;
        }
    }
}