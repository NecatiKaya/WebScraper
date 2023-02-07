using Quartz;
using WebScraper.Api.V2.Business;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.HttpClients;
using WebScraper.Api.V2.HttpClients.Puppeteer;
using WebScraper.Api.V2.Logging;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Jobs;

public class LoadCookiesJob : IJob, IDisposable
{
    private readonly ILogger<LoadCookiesJob> _logger;

    private readonly WebScraperDbContext _dbContext;

    private bool disposedValue;

    public LoadCookiesJob(ILogger<LoadCookiesJob> logger, WebScraperDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime start = DateTime.Now;
        string jobId = Guid.NewGuid().ToString();
        string jobName = nameof(LoadCookiesJob);
        string transactionId = Guid.NewGuid().ToString();

        WebScraperDbContext logDb = new WebScraperDbContext();
        ApplicationLogModelJar logJar = new ApplicationLogModelJar(logDb, _logger);
        logJar.JobName = jobName;
        logJar.JobId = jobId;
        logJar.LogAdded += LogJar_LogAdded;

        await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateInformationLog($"LoadCookiesJob is started at '{start.ToString()}'.", jobName, jobId, transactionId, start)));

        try
        {
            CrawlerHttpClientBase crawlerHttp = new PuppeterHttpClient(new ClientConfiguration()
            {
                Logger = _logger,
                LoggingJar = logJar
            });
            await crawlerHttp.ConfigureAsync();
            HttpClientResponse? response = await crawlerHttp.CrawlAsync(new Product("AmazonHomePage", "barcode", "ASIN", "https://www.trendyol.com/", "https://www.amazon.com.tr/"));
            if (response.HasSuccessFullStatusCode() && response.Value.Cookies is not null)
            {
                CookieStoreRepository repository = new CookieStoreRepository(_dbContext);
                CookieStore[] cookies = response.Value.Cookies.Select(cookie => new CookieStore(cookie.Value)).ToArray();
                await repository.SaveCookiesAsync(cookies);
                await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateInformationLog($"LoadCookiesJob cookies saved.", jobName, jobId, transactionId, start)));
            }
            else
            {
                await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateInformationLog($"LoadCookiesJob couldn't get any cookies or exception occured.", jobName, jobId, transactionId, start)));
            }

            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateInformationLog($"LoadCookiesJob has finished as at '{DateTime.Now.ToString()}'.", jobName, jobId, transactionId, start)));
            _logger.Log(LogLevel.Information, $"LoadCookiesJob has finished.");
        }
        catch (PuppeteerCrawlCookieException ex)
        {
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(ApplicationLogBusiness.CreateErrorLogFromException("PuppeteerCrawlCookieException is occured in LoadCookiesJob", jobName, jobId, transactionId, ex, start, DateTime.Now)));
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
            ApplicationLog jobEndInformationLog = ApplicationLogBusiness.CreateInformationLog($"LoadCookiesJob is finished at {DateTime.Now.ToString()}.", jobName, jobId, transactionId, finish, finish - start);
            await logJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(jobEndInformationLog));
            await logJar.SaveAppLogsIfNeededAsync(true);
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