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

    private Crawler(IFlurlClientFactory flurlClientFac, ClientConfiguration clientConfiguration, WebScraperDbContext context)
    {
        _flurlClientFac = flurlClientFac;
        _clientConfiguration = clientConfiguration;
        _dbContext = context;
    }

    public static Crawler Create(CrawlingOptions options, IFlurlClientFactory flurlClientFac, ClientConfiguration clientConfiguration, WebScraperDbContext context)
    {
        return new Crawler(flurlClientFac, clientConfiguration, context);
    }

    public async Task<(HttpClientResponse?, HttpClientResponse?)> CrawlAsync(Product product)
    {
        DateTime start = DateTime.Now;
        ApplicationLogModelJar? logModelJar = _clientConfiguration?.LoggingJar;

        try
        {
            CrawlerHttpClientBase amazonCrawler = new AmazonFlurlHttpClient(_flurlClientFac, _clientConfiguration, _dbContext);
            CrawlerHttpClientBase trendyolCrawler = new TrendyolFlurlHttpClient(_flurlClientFac, _clientConfiguration, _dbContext);

            amazonCrawler.Configure();
            trendyolCrawler.Configure();

            HttpClientResponse? amazonResponse = await amazonCrawler.CrawlAsync(product);
            HttpClientResponse? trendyolResponse = await trendyolCrawler.CrawlAsync(product);

            DateTime nowDate = DateTime.Now;
            int? visitId = amazonResponse?.ScraperVisitId;

            ApplicationLog infoLog = ApplicationLogBusiness.CreateInformationLog($"Product '{product.Name}' scraped.", "jobname", "jobid", "tranid", nowDate, nowDate - start, productId: product.Id,
                requestId: amazonCrawler.GetRequestId() + "/" + trendyolCrawler.GetRequestId(), visitId: visitId);

            if (logModelJar != null)
            {
                await logModelJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(infoLog));
            }

            IHtmlParser? amazonParser = amazonResponse.HasValue && amazonResponse.Value.ContentHtml is not null ? new AmazonParser(amazonResponse.Value.ContentHtml) : null;
            IHtmlParser? trendyolParser = trendyolResponse.HasValue && trendyolResponse.Value.ContentHtml is not null ? new TrendyolParser(trendyolResponse.Value.ContentHtml) : null;

            ProductPriceInformation? amazonPrice = amazonParser?.Parse();
            ProductPriceInformation? trendyolPrice = trendyolParser?.Parse();

            await UpdatePriceInformationAsync(product, visitId, amazonPrice, trendyolPrice);

            return (amazonResponse, trendyolResponse);
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

        return (null, null);
    }

    private async Task UpdatePriceInformationAsync(Product product, int? scraperVisitId, ProductPriceInformation? amazonPriceInformation, ProductPriceInformation? trendyolPriceInformation)
    {
        if (product == null || scraperVisitId == null)
        {
            return;
        }

        ScraperVisitRepository visitRepository = new ScraperVisitRepository(_dbContext);
        ScraperVisit? visit = await visitRepository.GetVisitById(scraperVisitId.Value);

        if (visit == null)
        {
            return;
        }

        if (visit.TrendyolPriceNotFoundReason != PriceNotFoundReasons.ExceptionOccured)
        {
            visit.TrendyolPriceNotFoundReason = PriceNotFoundReasons.PriceIsNotOnThePage;
        }

        if (visit.AmazonPriceNotFoundReason != PriceNotFoundReasons.ExceptionOccured && visit.AmazonPriceNotFoundReason != PriceNotFoundReasons.BotDetected)
        {
            visit.AmazonPriceNotFoundReason = PriceNotFoundReasons.PriceIsNotOnThePage;
        }

        if (trendyolPriceInformation is not null)
        {
            visit.TrendyolCurrentDiscountAsAmount = trendyolPriceInformation.CurrentDiscountAsAmount;
            visit.TrendyolCurrentDiscountAsPercentage = trendyolPriceInformation.CurrentDiscountAsPercentage;
            visit.TrendyolCurrentPrice = trendyolPriceInformation.CurrentPrice;
            visit.TrendyolPreviousPrice = trendyolPriceInformation.PreviousPrice;
            visit.TrendyolPriceNotFoundReason = PriceNotFoundReasons.Found;
        }
        if (amazonPriceInformation is not null)
        {
            visit.AmazonCurrentDiscountAsAmount = amazonPriceInformation.CurrentDiscountAsAmount;
            visit.AmazonCurrentDiscountAsPercentage = amazonPriceInformation.CurrentDiscountAsPercentage;
            visit.AmazonCurrentPrice = amazonPriceInformation.CurrentPrice;
            visit.AmazonPreviousPrice = amazonPriceInformation.PreviousPrice;
            visit.AmazonPriceNotFoundReason = PriceNotFoundReasons.Found;
        }

        if (trendyolPriceInformation is not null && amazonPriceInformation is not null)
        {
            bool needToNotify = false;

            decimal? calculatedDifferenceWithAmount = trendyolPriceInformation.CurrentPrice - amazonPriceInformation.CurrentPrice;
            decimal? calculatedDifferenceWithPercentage = calculatedDifferenceWithAmount * 100 / trendyolPriceInformation.CurrentPrice;

            if (product.RequestedPriceDifferenceWithAmount is not null)
            {
                needToNotify = calculatedDifferenceWithAmount >= product.RequestedPriceDifferenceWithAmount;
            }

            if (product.RequestedPriceDifferenceWithPercentage is not null)
            {
                needToNotify = calculatedDifferenceWithPercentage >= product.RequestedPriceDifferenceWithPercentage;
            }

            visit.CalculatedPriceDifferenceAsPercentage = calculatedDifferenceWithPercentage;
            visit.CalculatedPriceDifferenceAsAmount = calculatedDifferenceWithAmount;
            visit.RequestedPriceDifferenceAsPercentage = product!.RequestedPriceDifferenceWithPercentage;
            visit.RequestedPriceDifferenceAsAmount = product!.RequestedPriceDifferenceWithAmount;
            visit.NeedToNotify = needToNotify;
        }

        await visitRepository.UpdateVisit();
    }
}