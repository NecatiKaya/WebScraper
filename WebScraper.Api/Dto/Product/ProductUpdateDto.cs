namespace WebScraper.Api.Dto.Product
{
    public class ProductUpdateDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Barcode { get; set; }

        public string? ASIN { get; set; }

        public string? TrendyolUrl { get; set; }

        public string? AmazonUrl { get; set; }

        public decimal? RequestedPriceDifferenceWithAmount { get; set; }

        public decimal? RequestedPriceDifferenceWithPercentage { get; set; }

        public Data.Models.Product ToProduct()
        {
            Data.Models.Product p = new Data.Models.Product()
            {
                Id = Id,
                AmazonUrl = AmazonUrl!,
                ASIN = ASIN!,
                Barcode = Barcode!,
                Name = Name!,
                RequestedPriceDifferenceWithAmount = RequestedPriceDifferenceWithAmount,
                RequestedPriceDifferenceWithPercentage = RequestedPriceDifferenceWithPercentage,
                TrendyolUrl = TrendyolUrl!,
            };

            return p;
        }
    }
}
