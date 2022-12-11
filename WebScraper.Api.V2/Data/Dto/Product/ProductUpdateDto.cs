﻿namespace WebScraper.Api.V2.Data.Dto.Product;

public class ProductUpdateDto
{
    public ProductUpdateDto(string name, string barcode, string asin, string trendyolUrl, string amazonUrl)
    {
        Name = name;
        Barcode = barcode;
        ASIN = asin;
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

    public Models.Product ToProduct()
    {
        Models.Product p = new Models.Product(Name, Barcode, ASIN, TrendyolUrl, AmazonUrl)
        {
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