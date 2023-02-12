using Quartz;
using WebScraper.Api.V2.Business;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.HttpClients;
using WebScraper.Api.V2.HttpClients.Puppeteer;
using WebScraper.Api.V2.Logging;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Jobs;

public class CrawlJobWithPuppeteerOnlyAmazon : IJob, IDisposable
{
    private readonly ILogger<CrawlJobWithPuppeteerOnlyAmazon> _logger;

    private readonly WebScraperDbContext _dbContext;

    private bool disposedValue;

    public CrawlJobWithPuppeteerOnlyAmazon(ILogger<CrawlJobWithPuppeteerOnlyAmazon> logger, WebScraperDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime start = DateTime.Now;
        string jobId = Guid.NewGuid().ToString();
        string jobName = nameof(CrawlJobWithPuppeteerOnlyAmazon);
        string transactionId = Guid.NewGuid().ToString();

        WebScraperLogDbContext logDb = new WebScraperLogDbContext();
        ApplicationLogModelJar logJar = new ApplicationLogModelJar(logDb, _logger);
        logJar.JobName = jobName;
        logJar.JobId = jobId;
        logJar.LogAdded += LogJar_LogAdded;

        await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateInformationLog($"CrawlJobWithPuppeteerOnlyAmazon is started at '{start.ToString()}'.", jobName, jobId, transactionId, start)));

        try
        {
            List<HttpClientResponse> responses = new List<HttpClientResponse>();

            ProductRepository productRepo = new ProductRepository(_dbContext);
            RepositoryResponseBase<Product> productData = await productRepo.GetActiveProductsAsync();
            Product[] data = productData.Data ?? Array.Empty<Product>();

            CrawlerHttpClientBase crawlerHttp = new PuppeterHttpClient(new ClientConfiguration()
            {
                Logger = _logger,
                LoggingJar = logJar
            });
            await crawlerHttp.ConfigureAsync();

            int batchSize = 50;
            int numberOfBatches = (int)Math.Ceiling((double)data.Length / batchSize);

            for (int i = 0; i < numberOfBatches; i++)
            {
                List<Product> productsPerTask = data.Skip(i * batchSize).Take(batchSize).ToList();
                if (productsPerTask is null)
                {
                    continue;
                }

                var tasksForEachProduct = productsPerTask.Select(async eachProduct =>
                {
                    HttpClientResponse? response = await crawlerHttp.CrawlAsync(eachProduct);
                    return response;
                });
                HttpClientResponse?[]? allResults = await Task.WhenAll(tasksForEachProduct);
                if (allResults != null && allResults.Any())
                {
                    responses.AddRange((IEnumerable<HttpClientResponse>)allResults.Where(x => x != null).Select(x=>x));
                }                
            }
            
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateInformationLog($"CrawlJobWithPuppeteerOnlyAmazon has finished as at '{DateTime.Now.ToString()}'.", jobName, jobId, transactionId, start)));
            _logger.Log(LogLevel.Information, $"CrawlJobWithPuppeteerOnlyAmazon has finished.");
        }
        catch (PuppeteerDirectoryAccessException ex)
        {
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateErrorLogFromException("PuppeteerDirectoryAccessException is occured in LoadCookiesJob", jobName, jobId, transactionId, ex, start, DateTime.Now)));
        }
        catch (PuppeteerDownloadException ex)
        {
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateErrorLogFromException("PuppeteerDownloadException is occured in LoadCookiesJob", jobName, jobId, transactionId, ex, start, DateTime.Now)));
        }
        catch (PuppeteerExecutablePathException ex)
        {
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateErrorLogFromException("PuppeteerExecutablePathException is occured in LoadCookiesJob", jobName, jobId, transactionId, ex, start, DateTime.Now)));
        }
        catch (Exception ex)
        {
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateErrorLogFromException("Exception is occured in LoadCookiesJob", jobName, jobId, transactionId, ex, start, DateTime.Now)));
        }
        finally
        {
            DateTime finish = DateTime.Now;
            ApplicationLog jobEndInformationLog = ApplicationLogBusiness.CreateInformationLog($"CrawlJobWithPuppeteerOnlyAmazon is finished at {DateTime.Now.ToString()}.", jobName, jobId, transactionId, finish, finish - start);
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(jobEndInformationLog), force: true);
            logJar.LogAdded -= LogJar_LogAdded;
            if (logDb is not null)
            {
                await logDb.DisposeAsync();
            }
        }
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
