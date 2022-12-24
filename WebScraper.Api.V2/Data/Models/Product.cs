namespace WebScraper.Api.V2.Data.Models;

public class Product
{
    public Product(string name, string barcode, string aSIN, string trendyolUrl, string amazonUrl)
    {
        ScraperVisits = new List<ScraperVisit>();
        Name = name;
        Barcode = barcode;
        ASIN = aSIN;
        TrendyolUrl = trendyolUrl;
        AmazonUrl = amazonUrl;
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public string Barcode { get; set; }

    public string ASIN { get; set; }

    public string TrendyolUrl { get; set; }

    public string AmazonUrl { get; set; }

    public decimal? RequestedPriceDifferenceWithAmount { get; set; }

    public decimal? RequestedPriceDifferenceWithPercentage { get; set; }

    public bool IsDeleted { get; set; } = false;

    public List<ScraperVisit> ScraperVisits { get; set; }
}