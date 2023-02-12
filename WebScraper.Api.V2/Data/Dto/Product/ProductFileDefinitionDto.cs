namespace WebScraper.Api.V2.Data.Dto.Product;

public class ProductFileDefinitionDto
{
    public string? Brand { get; set; }

    public string? Name { get; set; }

    public string? Barcode { get; set; }

    public string? Asin { get; set; }

    public string? AmazonLink { get; set; }

    public string? TrendyolLink { get; set; }

    public decimal? Percentage { get; set; }

    public bool IsValid()
    {
        return Brand is not null && Brand.Length > 0 &&
            Name is not null && Name.Length > 5 &&
            Barcode is not null && Barcode.Length > 5 &&
            Asin is not null && Asin.Length > 5 &&
            AmazonLink is not null && AmazonLink.Length > 5 &&
            TrendyolLink is not null && TrendyolLink.Length > 5 &&
            Percentage is not null;
    }
}