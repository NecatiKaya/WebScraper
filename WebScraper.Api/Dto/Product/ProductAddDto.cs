using WebScraper.Api.Data.Models;

namespace WebScraper.Api.Dto.Product
{
    public class ProductAddDto
    {
        public string? Name { get; set; }

        public string? Barcode { get; set; }

        public string? ASIN { get; set; }

        public string? TrendyolUrl { get; set; }

        public string? AmazonUrl { get; set; }

        public decimal? RequestedPriceDiffrenceWithAmount { get; set; }

        public decimal? RequestedPriceDiffrenceWithPercentage { get; set; }

        public bool IsDeleted { get; set; } = false;

        public Data.Models.Product ToProduct()
        {
            Data.Models.Product p = new Data.Models.Product()
            {
                AmazonUrl = AmazonUrl!,
                ASIN = ASIN!,
                Barcode = Barcode!,
                IsDeleted = IsDeleted!,
                Name = Name!,
                RequestedPriceDifferenceWithAmount = RequestedPriceDiffrenceWithAmount,
                RequestedPriceDifferenceWithPercentage = RequestedPriceDiffrenceWithPercentage,
                TrendyolUrl = TrendyolUrl!,
            };

            return p;
        }
    }
}
