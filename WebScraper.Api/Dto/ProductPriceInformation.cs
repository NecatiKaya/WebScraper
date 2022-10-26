namespace WebScraper.Api.Dto
{
    public class ProductPriceInformation
    {
        public ProductPriceInformation()
        {

        }

        public decimal PreviousPrice { get; set; } = 0;

        public decimal CurrentPrice { get; set; } = 0;

        public decimal CurrentDiscountAsAmount { get; set; } = 0;

        public decimal CurrentDiscountAsPercentage { get; set; } = 0;
    }
}
