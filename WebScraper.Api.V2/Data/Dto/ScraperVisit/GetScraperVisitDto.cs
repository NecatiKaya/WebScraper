using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Data.Dto.ScraperVisit;

public class GetScraperVisitDto
{
    public int VisitId { get; set; }

    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public DateTime? VisitDate { get; set; }

    public decimal? AmazonPreviousPrice { get; set; }

    public decimal? AmazonCurrentPrice { get; set; }

    public decimal? AmazonCurrentDiscountAsAmount { get; set; }

    public decimal? AmazonCurrentDiscountAsPercentage { get; set; }

    public decimal? TrendyolPreviousPrice { get; set; }

    public decimal? TrendyolCurrentPrice { get; set; }

    public decimal? TrendyolCurrentDiscountAsAmount { get; set; }

    public decimal? TrendyolCurrentDiscountAsPercentage { get; set; }

    public decimal? CalculatedPriceDifferenceAsAmount { get; set; }

    public decimal? CalculatedPriceDifferenceAsPercentage { get; set; }

    public decimal? RequestedPriceDifferenceAsAmount { get; set; }

    public decimal? RequestedPriceDifferenceAsPercentage { get; set; }

    public bool NeedToNotify { get; set; } = false;

    public bool Notified { get; set; } = false;

    public int? LogId { get; set; }

    public string? UsedCookieValue { get; set; }

    public string? UsedUserAgentValue { get; set; }
    public PriceNotFoundReasons? AmazonPriceNotFoundReason { get; set; }

    public PriceNotFoundReasons? TrendyolPriceNotFoundReason { get; set; }
}