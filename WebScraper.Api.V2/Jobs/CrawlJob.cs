using Flurl.Http.Configuration;
using Quartz;
using WebScraper.Api.V2.Business;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.HttpClients;
using WebScraper.Api.V2.Logging;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Jobs;

public class CrawlJob : IJob, IDisposable
{
    private readonly ILogger<CrawlJob> _logger;

    private readonly WebScraperDbContext _dbContext;

    private readonly AlertingBusiness _alertingBusiness;

    private readonly IFlurlClientFactory _flurlClientFac;

    private bool disposedValue;

    public CrawlJob(ILogger<CrawlJob> logger, WebScraperDbContext dbContext, 
        AlertingBusiness alertingBusiness, IFlurlClientFactory flurlClientFac)
    {
        _logger = logger;
        _dbContext = dbContext;
        _alertingBusiness = alertingBusiness;
        _flurlClientFac = flurlClientFac;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime start = DateTime.Now;
        string jobId = Guid.NewGuid().ToString();
        string jobName = nameof(CrawlJob);
        string transactionId = Guid.NewGuid().ToString();

        WebScraperLogDbContext logDbGlobal = new WebScraperLogDbContext();
        ApplicationLogModelJar logJar = new ApplicationLogModelJar(logDbGlobal, _logger);
        logJar.JobName = jobName;
        logJar.JobId = jobId;
        logJar.TransactionId = transactionId;
        logJar.LogAdded += LogJar_LogAdded;

        CrawlingOptions options = new CrawlingOptions() { RetryCountOnFail = 3 };
        ClientConfiguration configuration = new ClientConfiguration()
        {
            Logger = _logger,
            LoggingJar = logJar
        };

        //Crawler crawler = Crawler.Create(options, _flurlClientFac, configuration, _dbContext, logDb);

        try
        {
            ProductRepository productRepo = new ProductRepository(_dbContext);
            RepositoryResponseBase<Product> productData = await productRepo.GetActiveProductsAsync();
            Product[] data = productData.Data ?? Array.Empty<Product>();
            //foreach (Product eachProduct in data)
            //{
            //    ScraperVisit visit = await crawler.CrawlAsync(eachProduct);
            //}

            //await _alertingBusiness.SendPriceAlertEmail();


            //await ProcessInParallel(crawler, data);

            int batchSize = 50;
            int numberOfBatches = (int)Math.Ceiling((double)data.Length / batchSize);

            for (int i = 0; i < numberOfBatches; i++)
            {
                List<Product> productsPerTask = data.Skip(i * batchSize).Take(batchSize).ToList();
                if (productsPerTask is null)
                {
                    continue;
                }

                var tasksForEachProduct = productsPerTask.Select(eachProduct =>
                {
                    WebScraperDbContext context = new WebScraperDbContext();
                    WebScraperLogDbContext logDb = new WebScraperLogDbContext();
                    ApplicationLogModelJar logJar = new ApplicationLogModelJar(logDb, _logger);
                    logJar.JobName = jobName;
                    logJar.JobId = jobId;
                    logJar.TransactionId = transactionId;
                    logJar.LogAdded += LogJar_LogAdded;

                    CrawlingOptions options = new CrawlingOptions() { RetryCountOnFail = 3 };
                    ClientConfiguration configuration = new ClientConfiguration()
                    {
                        Logger = _logger,
                        LoggingJar = logJar
                    };

                    Crawler crawler = new Crawler(_flurlClientFac, configuration, context, logDb, eachProduct);
                    return crawler.CrawlAsync();
                });
                await Task.WhenAll(tasksForEachProduct);
            }
            await _alertingBusiness.SendPriceAlertEmail();            

            DateTime end = DateTime.Now;
            TimeSpan difference = end - start;

            ApplicationLog appLog = ApplicationLogBusiness.CreateInformationLog($"Crawl job duration as miliseconds :'{difference.Milliseconds}'", jobName, jobId, transactionId, DateTime.Now, difference);
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(appLog));
        }
        catch (Exception ex)
        {
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateErrorLogFromException("Exception is occured in CrawlJob", jobName, jobId, transactionId, ex, start, DateTime.Now)));
        }
        finally
        {
            DateTime finish = DateTime.Now;
            ApplicationLog jobEndInformationLog = ApplicationLogBusiness.CreateInformationLog($"{nameof(CrawlJob)} is finished at {DateTime.Now.ToString()}.", jobName, jobId, transactionId, finish, finish - start);
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(jobEndInformationLog), force: true);
            //await logJar.SaveAppLogsIfNeededAsync(true);
            logJar.LogAdded -= LogJar_LogAdded;
            if (logDbGlobal is not null)
            {
                await logDbGlobal.DisposeAsync();
            }
            _flurlClientFac?.Dispose();
        }
    }

    //private async Task<ScraperVisit?[]> ProcessInParallel(Crawler crawler, Product[] products)
    //{
    //    var tasks = new List<Task<ScraperVisit?>>();
    //    int numberOfRequests = products.Length;
    //    int maxParallelRequests = numberOfRequests;
    //    var semaphoreSlim = new SemaphoreSlim(maxParallelRequests, maxParallelRequests);

    //    for (int i = 0; i < numberOfRequests; ++i)
    //    {
    //        tasks.Add(CrawlProductWithSemaphore(crawler, products[i], semaphoreSlim));
    //    }

    //    return await Task.WhenAll(tasks.ToArray());
    //}

    //private async Task<ScraperVisit?> CrawlProductWithSemaphore(Crawler crawler, Product product, SemaphoreSlim semaphore)
    //{
    //    await semaphore.WaitAsync();
    //    ScraperVisit? visit = await crawler.CrawlAsync(product);
    //    //Thread.Sleep(10000);
    //    semaphore.Release();
    //    return visit;
    //}

    private void LogJar_LogAdded(object sender, ApplicationLogModelEventArgs e)
    {
        ApplicationLogModelJar jar = (sender as ApplicationLogModelJar)!;
        ApplicationLog appLog = e.ApplicationLogModel.AppLog;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _dbContext?.Dispose();
                _flurlClientFac?.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}