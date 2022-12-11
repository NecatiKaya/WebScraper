namespace WebScraper.Api.V2.Data.Dto;

public class ProductPriceInformation
{
    public decimal? PreviousPrice { get; set; }

    public decimal? CurrentPrice { get; set; }

    public decimal? CurrentDiscountAsAmount { get; set; }

    public decimal? CurrentDiscountAsPercentage { get; set; }
}