using System.Diagnostics;
using WebScraper.Api.Business.Parsers;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Business;

public class CrawlingBusiness
{
    public async Task<List<ScraperVisit>> CrawlProducts(List<Product> products)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        List<Task<HtmlInfos>> tasks = new List<Task<HtmlInfos>>();
        IEnumerable<Task<HtmlInfos>> crawlTasks = products.Select(_product => CrawlProduct(_product));
        tasks.AddRange(crawlTasks);

        IEnumerable<HtmlInfos> allHtmls = (await Task.WhenAll(tasks));
        List<ScraperVisit> visits = new List<ScraperVisit>(products.Count);
        foreach (HtmlInfos eachHtmlInfos in allHtmls)
        {
            IHtmlParser trendyolParser = new TrendyolParser(eachHtmlInfos.TrendyolHtmlInfo?.Html!);
            IHtmlParser amazonParser = new AmazonParser(eachHtmlInfos.AmazonHtmlInfo?.Html!);

            ProductPriceInformation? trendyolPriceInformation = trendyolParser.Parse();
            ProductPriceInformation? amazonPriceInformation = amazonParser.Parse();

            ScraperVisit visit = new ScraperVisit()
            {
                ProductId = eachHtmlInfos!.Product!.Id!,
                VisitDate = DateTime.Now,
                Notified = false
            };
            if (trendyolPriceInformation is not null)
            {
                visit.TrendyolCurrentDiscountAsAmount = trendyolPriceInformation.CurrentDiscountAsAmount;
                visit.TrendyolCurrentDiscountAsPercentage = trendyolPriceInformation.CurrentDiscountAsPercentage;
                visit.TrendyolCurrentPrice = trendyolPriceInformation.CurrentPrice;
                visit.TrendyolPreviousPrice = trendyolPriceInformation.PreviousPrice;
            }

            if (amazonPriceInformation is not null)
            {
                visit.AmazonCurrentDiscountAsAmount = amazonPriceInformation.CurrentDiscountAsAmount;
                visit.AmazonCurrentDiscountAsPercentage = amazonPriceInformation.CurrentDiscountAsPercentage;
                visit.AmazonCurrentPrice = amazonPriceInformation.CurrentPrice;
                visit.AmazonPreviousPrice = amazonPriceInformation.PreviousPrice;
            }

            if (trendyolPriceInformation is not null && amazonPriceInformation is not null)
            {
                decimal calculatedDifferenceWithPercentage = 0;
                decimal calculatedDifferenceWithAmount = 0;
                bool needToNotify = false;

                calculatedDifferenceWithAmount = trendyolPriceInformation.CurrentPrice - amazonPriceInformation.CurrentPrice;
                calculatedDifferenceWithPercentage = calculatedDifferenceWithAmount * 100 / trendyolPriceInformation.CurrentPrice;

                if (eachHtmlInfos.Product.RequestedPriceDifferenceWithAmount is not null)
                {
                    needToNotify = calculatedDifferenceWithAmount >= eachHtmlInfos.Product.RequestedPriceDifferenceWithAmount;
                }

                if (eachHtmlInfos.Product.RequestedPriceDifferenceWithPercentage is not null)
                {
                    needToNotify = calculatedDifferenceWithPercentage >= eachHtmlInfos.Product.RequestedPriceDifferenceWithPercentage;
                }

                visit.CalculatedPriceDifferenceAsPercentage = calculatedDifferenceWithPercentage;
                visit.CalculatedPriceDifferenceAsAmount = calculatedDifferenceWithAmount;
                visit.RequestedPriceDifferenceAsPercentage = eachHtmlInfos!.Product!.RequestedPriceDifferenceWithPercentage;
                visit.RequestedPriceDifferenceAsAmount = eachHtmlInfos!.Product!.RequestedPriceDifferenceWithAmount;
                visit.NeedToNotify = needToNotify;
            }

            visits.Add(visit);
        }

        stopwatch.Stop();
        Console.WriteLine("Elapsed.............");
        Console.WriteLine(stopwatch.Elapsed);
        return visits;
    }

    public async Task<HtmlInfos> CrawlProduct(Product product)
    {
        HtmlInfos htmls = new();
        htmls.TrendyolHtmlInfo = await HtmlInfo.GetHtml(product.TrendyolUrl, Websites.Trendyol);
        htmls.AmazonHtmlInfo = await HtmlInfo.GetHtml(product.AmazonUrl, Websites.Amazon);
        htmls.Product = product;
        return htmls;
    }
}