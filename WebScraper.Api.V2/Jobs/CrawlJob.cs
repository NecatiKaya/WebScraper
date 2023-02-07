using Flurl.Http.Configuration;
using Quartz;
using System.Threading;
using WebScraper.Api.V2.Business;
using WebScraper.Api.V2.Business.Email;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.HttpClients;
using WebScraper.Api.V2.Logging;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Jobs;

public class CrawlJob : IJob, IDisposable
{
    private readonly ILogger<CrawlJob> _logger;

    private readonly WebScraperDbContext _dbContext;

    private readonly IMailSender _mailSender;

    private readonly IFlurlClientFactory _flurlClientFac;

    private bool disposedValue;

    public CrawlJob(ILogger<CrawlJob> logger, WebScraperDbContext dbContext, IMailSender mailSender, IFlurlClientFactory flurlClientFac, WebScraperDbContext context)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mailSender = mailSender;
        _flurlClientFac = flurlClientFac;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime start = DateTime.Now;
        string jobId = Guid.NewGuid().ToString();
        string jobName = nameof(CrawlJob);
        string transactionId = Guid.NewGuid().ToString();

        WebScraperDbContext logDb = new WebScraperDbContext();
        ApplicationLogModelJar logJar = new ApplicationLogModelJar(logDb, _logger);
        logJar.JobName = jobName;
        logJar.JobId = jobId;
        logJar.TransactionId = transactionId;
        logJar.LogAdded += LogJar_LogAdded;

        CrawlingOptions options = new CrawlingOptions() { RetryCountOnFail = 3};
        ClientConfiguration configuration = new ClientConfiguration()
        {
            Logger = _logger,
            LoggingJar = logJar
        };
        Crawler crawler = Crawler.Create(options, _flurlClientFac, configuration, _dbContext);

        try
        {
            ProductRepository productRepo = new ProductRepository(_dbContext);
            RepositoryResponseBase<Product> productData = await productRepo.GetActiveProductsAsync();
            Product[] data = productData.Data ?? new Product[0];
            //foreach (Product eachProduct in data)
            //{
            //    var responses = await crawler.CrawlAsync(eachProduct);
            //}
            await ProcessInParallel(crawler, data);

            DateTime end = DateTime.Now;
            TimeSpan difference = end - start;
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
            if (logDb is not null)
            {
                await logDb.DisposeAsync();
            }
            _flurlClientFac?.Dispose();
        }
    }

    private async Task<(HttpClientResponse?, HttpClientResponse?)[]?> ProcessInParallel(Crawler crawler, Product[] products)
    {
        var tasks = new List<Task<(HttpClientResponse?, HttpClientResponse?)>>();
        int numberOfRequests = products.Length;
        int maxParallelRequests = 1;
        var semaphoreSlim = new SemaphoreSlim(maxParallelRequests, maxParallelRequests);

        for (int i = 0; i < numberOfRequests; ++i)
        {
            tasks.Add(CrawlProductWithSemaphore(crawler, products[i], semaphoreSlim));
        }

        return await Task.WhenAll(tasks.ToArray());
    }

    private async Task<(HttpClientResponse?, HttpClientResponse?)> CrawlProductWithSemaphore(Crawler crawler, Product product, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        var response = await crawler.CrawlAsync(product);
        //Thread.Sleep(10000);
        semaphore.Release();
        return response;
    }

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