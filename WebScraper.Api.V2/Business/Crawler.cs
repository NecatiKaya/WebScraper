using Flurl.Http.Configuration;
using WebScraper.Api.V2.Business.Parsing;
using WebScraper.Api.V2.Data.Dto;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.HttpClients;
using WebScraper.Api.V2.HttpClients.Flurl;
using WebScraper.Api.V2.Logging;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Business;

public class Crawler
{
    private readonly IFlurlClientFactory _flurlClientFac;
    private readonly ClientConfiguration _clientConfiguration;
    private readonly WebScraperDbContext _dbContext;
    private readonly WebScraperLogDbContext _logDbContext;
    private readonly Product _product;

    public Crawler(IFlurlClientFactory flurlClientFac, ClientConfiguration clientConfiguration, WebScraperDbContext dbContext, WebScraperLogDbContext logDbContext, Product product)
    {
        _flurlClientFac = flurlClientFac;
        _clientConfiguration = clientConfiguration;
        _dbContext = dbContext;
        _logDbContext = logDbContext;
        _product = product;
    }

    //public static Crawler Create(IFlurlClientFactory _flurlClientFac, ClientConfiguration _clientConfiguration, WebScraperDbContext _dbContext, WebScraperLogDbContext _logDbContext)
    //{
    //    return new Crawler(_flurlClientFac, _clientConfiguration, _dbContext, _logDbContext);
    //}

    public async Task<ScraperVisit?> CrawlAsync()
    {
        var crawlResponse = await InnerCrawlAsync();

        if (crawlResponse.Item1 != null && crawlResponse.Item1.AmazonPriceNotFoundReason == PriceNotFoundReasons.BotDetected) 
        {
            crawlResponse = await ReRunAmazon(crawlResponse.Item3);
        }

        if (crawlResponse.Item1 != null && crawlResponse.Item1.AmazonPriceNotFoundReason == PriceNotFoundReasons.BotDetected)
        {
            crawlResponse = await ReRunAmazon(crawlResponse.Item3);
        }

        //Trendyol TooMany Request Ex Handling. We will not try because it will be available after some time
        //if (crawlResponse.Item1 != null && crawlResponse.Item1.TrendyolPriceNotFoundReason == PriceNotFoundReasons.BotDetected)
        //{
        //    crawlResponse = await InnerCrawlAsync(crawlResponse.Item2, null);
        //    visits.Add(crawlResponse.Item1);
        //}

        //if (crawlResponse.Item1 != null && crawlResponse.Item1.TrendyolPriceNotFoundReason == PriceNotFoundReasons.BotDetected)
        //{
        //    crawlResponse = await InnerCrawlAsync(crawlResponse.Item2, null);
        //    visits.Add(crawlResponse.Item1);
        //}

        return crawlResponse.Item1;
    }

    private async Task<(ScraperVisit?, HttpClientResponse?, HttpClientResponse?)> InnerCrawlAsync(HttpClientResponse? existingAmazonResponse = null, HttpClientResponse? existingTrendyolResponse = null, 
        string? amazonCookie = null, string? trendyolCookie = null, string? userAgent = null)
    {
        DateTime start = DateTime.Now;
        ApplicationLogModelJar? logModelJar = _clientConfiguration?.LoggingJar;

        CrawlerHttpClientBase amazonCrawler = new AmazonFlurlHttpClient(_flurlClientFac, _clientConfiguration, _dbContext, _logDbContext);
        CrawlerHttpClientBase trendyolCrawler = new TrendyolFlurlHttpClient(_flurlClientFac, _clientConfiguration, _dbContext, _logDbContext);

        amazonCrawler.Configure();
        trendyolCrawler.Configure();

        ScraperVisitRepository scraperVisitRepository = new ScraperVisitRepository(_dbContext);
        ScraperVisit scraperVisit = new ScraperVisit()
        {
            NeedToNotify = false,
            Notified = false,
            ProductId = _product.Id,
            StartDate = DateTime.Now,
            JobId = logModelJar?.JobId
        };

        try
        {
            HttpClientResponse? amazonResponse = existingAmazonResponse;
            HttpClientResponse? trendyolResponse = existingTrendyolResponse;
            if (amazonResponse == null)
            {
                amazonResponse = await amazonCrawler.CrawlAsync(_product, amazonCookie, userAgent);
            }
            if (trendyolResponse == null)
            {
                trendyolResponse = await trendyolCrawler.CrawlAsync(_product, trendyolCookie, userAgent);
            }

            DateTime nowDate = DateTime.Now;
            scraperVisit.EndDate = nowDate;
            scraperVisit.RequestedPriceDifferenceAsAmount = _product.RequestedPriceDifferenceWithAmount;
            scraperVisit.RequestedPriceDifferenceAsPercentage = _product.RequestedPriceDifferenceWithPercentage;

            if (amazonResponse is not null)
            {
                if (amazonResponse.Value.IsBotDetected)
                {
                    scraperVisit.AmazonPriceNotFoundReason = PriceNotFoundReasons.BotDetected;
                }
                else
                {
                    scraperVisit.AmazonPriceNotFoundReason = amazonResponse.HasSuccessFullStatusCode() ? PriceNotFoundReasons.Initial : PriceNotFoundReasons.ExceptionOccured;
                }
            }
            else
            {
                scraperVisit.AmazonPriceNotFoundReason = PriceNotFoundReasons.ExceptionOccured;
            }

            if (trendyolResponse is not null)
            {
                if (trendyolResponse.Value.IsBotDetected)
                {
                    scraperVisit.TrendyolPriceNotFoundReason = PriceNotFoundReasons.TooManyRequest;
                }
                else
                {
                    scraperVisit.TrendyolPriceNotFoundReason = trendyolResponse.HasSuccessFullStatusCode() ? PriceNotFoundReasons.Initial : PriceNotFoundReasons.ExceptionOccured;
                }
            }
            else
            {
                scraperVisit.TrendyolPriceNotFoundReason = PriceNotFoundReasons.ExceptionOccured;
            }

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

                if (_product.RequestedPriceDifferenceWithAmount is not null)
                {
                    needToNotify = calculatedDifferenceWithAmount >= _product.RequestedPriceDifferenceWithAmount;
                }

                if (_product.RequestedPriceDifferenceWithPercentage is not null)
                {
                    needToNotify = calculatedDifferenceWithPercentage >= _product.RequestedPriceDifferenceWithPercentage;
                }

                scraperVisit.CalculatedPriceDifferenceAsPercentage = calculatedDifferenceWithPercentage;
                scraperVisit.CalculatedPriceDifferenceAsAmount = calculatedDifferenceWithAmount;
                scraperVisit.NeedToNotify = needToNotify;
            }

            await scraperVisitRepository.AddVisitsAsync(new ScraperVisit[] { scraperVisit });
            return (scraperVisit, amazonResponse, trendyolResponse);
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

        return (null, null, null);
    }

    private async Task<(ScraperVisit?, HttpClientResponse?, HttpClientResponse?)> ReRunAmazon(HttpClientResponse? trendyolResponse)
    {
        CookieStoreRepository cookieStoreRepository = new CookieStoreRepository(_logDbContext);
        UserAgentStringRepository userAgentStringRepository = new UserAgentStringRepository(_logDbContext);

        CookieStore? amazonCookie = await cookieStoreRepository.GetNotUsedCookieAsync(Websites.Amazon);
        UserAgentString ua = await userAgentStringRepository.GetRandomUserAgentAsync();
        (ScraperVisit?, HttpClientResponse?, HttpClientResponse?) crawlResponse = await InnerCrawlAsync(null, trendyolResponse, amazonCookie?.CookieValue, ua.Agent);
        return crawlResponse;
    }
}