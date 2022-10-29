using HtmlAgilityPack;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Business.ScrapingHandlers.Amazon.PriceHandlers;

public class AmazonNoPriceHandler : AmazonPriceHandlerBase
{
    public override ProductPriceInformation? HandleRequst(HtmlDocument doc)
    {
        HtmlNode? priceAvailability = doc.GetElementbyId("availability_feature_div");
        if (priceAvailability != null && priceAvailability.HasChildNodes)
        {
            bool? noPriceAvailable = priceAvailability.SelectNodes("//div[@id='availability']")?.FirstOrDefault()?.InnerHtml?.ToLowerInvariant().Contains("Şu anda mevcut değil".ToLowerInvariant());
            if (noPriceAvailable == true)
            {
                return null;
            }
        }

        return Successor?.HandleRequst(doc);
    }
}