using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Data.Dto.ScraperVisit;

public class NotNotifiedVisitDto
{
    public int Id { get; set; }

    public string? JobId { get; set; }

    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

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
}