using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using WebScraper.Api.V2.Business.Parsing;
using WebScraper.Api.V2.Data.Dto;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Exceptions;
using WebScraper.Api.V2.HttpClients;
using WebScraper.Api.V2.HttpClients.Flurl;
using WebScraper.Api.V2.Logging;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Business;

public class Crawler
{
    //private readonly IFlurlClientFactory _flurlClientFac;
    //private readonly ClientConfiguration _clientConfiguration;
    //private readonly WebScraperDbContext _dbContext;
    //private readonly WebScraperLogDbContext _logDbContext;
    
    //private Crawler(IFlurlClientFactory flurlClientFac, ClientConfiguration clientConfiguration, WebScraperDbContext context, WebScraperLogDbContext logDbContext)
    //{
    //    _flurlClientFac = flurlClientFac;
    //    _clientConfiguration = clientConfiguration;
    //    _dbContext = context;
    //    _logDbContext = logDbContext;
    //}

    //public static Crawler Create(IFlurlClientFactory flurlClientFac, ClientConfiguration clientConfiguration, WebScraperDbContext context, WebScraperLogDbContext logDbContext)
    //{
    //    return new Crawler(flurlClientFac, clientConfiguration, context, logDbContext);
    //}

    public async Task<ScraperVisit?> CrawlAsync(Product product, IFlurlClientFactory flurlClientFac, ClientConfiguration clientConfiguration, WebScraperDbContext context, WebScraperLogDbContext logDbContext)
    {
        DateTime start = DateTime.Now;
        ApplicationLogModelJar? logModelJar = clientConfiguration?.LoggingJar;

        CrawlerHttpClientBase amazonCrawler = new AmazonFlurlHttpClient(flurlClientFac, clientConfiguration, context, logDbContext);
        CrawlerHttpClientBase trendyolCrawler = new TrendyolFlurlHttpClient(flurlClientFac, clientConfiguration, context, logDbContext);

        amazonCrawler.Configure();
        trendyolCrawler.Configure();

        ScraperVisitRepository scraperVisitRepository = new ScraperVisitRepository(context);
        ScraperVisit scraperVisit = new ScraperVisit()
        {
            NeedToNotify = false,
            Notified = false,
            ProductId = product.Id,
            StartDate = DateTime.Now
        };

        try
        {
            HttpClientResponse? amazonResponse = await amazonCrawler.CrawlAsync(product);
            HttpClientResponse? trendyolResponse = await trendyolCrawler.CrawlAsync(product);

            DateTime nowDate = DateTime.Now;
            scraperVisit.EndDate = nowDate;
            scraperVisit.RequestedPriceDifferenceAsAmount = product.RequestedPriceDifferenceWithAmount;
            scraperVisit.RequestedPriceDifferenceAsPercentage = product.RequestedPriceDifferenceWithPercentage;

            if (amazonResponse is not null && amazonResponse.Value.IsBotDetected)
            {
                scraperVisit.AmazonPriceNotFoundReason = PriceNotFoundReasons.BotDetected;
            }
            else 
            {
                scraperVisit.AmazonPriceNotFoundReason = amazonResponse.HasSuccessFullStatusCode() ? PriceNotFoundReasons.Initial : PriceNotFoundReasons.ExceptionOccured;
            }
            
            scraperVisit.TrendyolPriceNotFoundReason = trendyolResponse.HasSuccessFullStatusCode() ? PriceNotFoundReasons.Initial : PriceNotFoundReasons.ExceptionOccured;

            IHtmlParser? amazonParser = amazonResponse.HasValue && amazonResponse.Value.ContentHtml is not null ? new AmazonParser(amazonResponse.Value.ContentHtml) : null;
            IHtmlParser? trendyolParser = trendyolResponse.HasValue && trendyolResponse.Value.ContentHtml is not null ? new TrendyolParser(trendyolResponse.Value.ContentHtml) : null;

            ProductPriceInformation? amazonPrice = amazonParser?.Parse();
            ProductPriceInformation? trendyolPrice = trendyolParser?.Parse();

            if (scraperVisit.AmazonPriceNotFoundReason != PriceNotFoundReasons.ExceptionOccured && scraperVisit.AmazonPriceNotFoundReason != PriceNotFoundReasons.BotDetected)
            {
                if (amazonPrice == null)
                {
                    scraperVisit.AmazonPriceNotFoundReason = PriceNotFoundReasons.PriceIsNotOnThePage;
                }
                else
                {
                    scraperVisit.AmazonCurrentDiscountAsAmount = amazonPrice.CurrentDiscountAsAmount;
                    scraperVisit.AmazonCurrentDiscountAsPercentage = amazonPrice.CurrentDiscountAsPercentage;
                    scraperVisit.AmazonCurrentPrice = amazonPrice.CurrentPrice;
                    scraperVisit.AmazonPreviousPrice = amazonPrice.PreviousPrice;
                    scraperVisit.AmazonPriceNotFoundReason = PriceNotFoundReasons.Found;
                }
            }

            if (scraperVisit.TrendyolPriceNotFoundReason != PriceNotFoundReasons.ExceptionOccured)
            {
                if (trendyolPrice == null)
                {
                    scraperVisit.TrendyolPriceNotFoundReason = PriceNotFoundReasons.PriceIsNotOnThePage;
                }
                else
                {
                    scraperVisit.TrendyolCurrentDiscountAsAmount = trendyolPrice.CurrentDiscountAsAmount;
                    scraperVisit.TrendyolCurrentDiscountAsPercentage = trendyolPrice.CurrentDiscountAsPercentage;
                    scraperVisit.TrendyolCurrentPrice = trendyolPrice.CurrentPrice;
                    scraperVisit.TrendyolPreviousPrice = trendyolPrice.PreviousPrice;
                    scraperVisit.TrendyolPriceNotFoundReason = PriceNotFoundReasons.Found;
                }
            }

            if (trendyolPrice is not null && amazonPrice is not null)
            {
                bool needToNotify = false;

                decimal? calculatedDifferenceWithAmount = trendyolPrice.CurrentPrice - amazonPrice.CurrentPrice;
                decimal? calculatedDifferenceWithPercentage = calculatedDifferenceWithAmount * 100 / trendyolPrice.CurrentPrice;

                if (product.RequestedPriceDifferenceWithAmount is not null)
                {
                    needToNotify = calculatedDifferenceWithAmount >= product.RequestedPriceDifferenceWithAmount;
                }

                if (product.RequestedPriceDifferenceWithPercentage is not null)
                {
                    needToNotify = calculatedDifferenceWithPercentage >= product.RequestedPriceDifferenceWithPercentage;
                }

                scraperVisit.CalculatedPriceDifferenceAsPercentage = calculatedDifferenceWithPercentage;
                scraperVisit.CalculatedPriceDifferenceAsAmount = calculatedDifferenceWithAmount;
                scraperVisit.NeedToNotify = needToNotify;
            }

            await scraperVisitRepository.AddVisitsAsync(new ScraperVisit[] { scraperVisit });

            return scraperVisit;
        }
        catch (Exception ex)
        {            
            if (logModelJar != null)
            {
                await logModelJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateErrorLogFromException("Exception is occured in Crawler.CrawlAsync",
                logModelJar.JobName ?? "#JOBNAME#NOT#DEFINED",
                logModelJar.JobId ?? "#JOBID#NOT#DEFINED",
                logModelJar.TransactionId,
                ex,
                start,
                DateTime.Now)));
            }
        }

        return null;
    }
}