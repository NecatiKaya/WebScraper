namespace WebScraper.Api.Data.Models
{
    public class Product
    {
        public Product()
        {
            ScraperVisits = new List<ScraperVisit>();
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
}
