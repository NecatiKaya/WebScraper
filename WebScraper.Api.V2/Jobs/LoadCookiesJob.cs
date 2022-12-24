using Quartz;
using WebScraper.Api.V2.Business;
using WebScraper.Api.V2.Business.Email;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.HttpClients;
using WebScraper.Api.V2.HttpClients.Puppeteer;
using WebScraper.Api.V2.Logging;

namespace WebScraper.Api.V2.Jobs;

public class LoadCookiesJob : IJob
{
    private readonly ILogger<LoadCookiesJob> _logger;

    private readonly WebScraperDbContext _dbContext;

    private readonly IMailSender _mailSender;

    public LoadCookiesJob(ILogger<LoadCookiesJob> logger, WebScraperDbContext dbContext, IMailSender mailSender)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mailSender = mailSender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime start = DateTime.Now;
        _logger.Log(LogLevel.Information, $"LoadCookiesJob {start.ToString()} is started.");

        string jobId = Guid.NewGuid().ToString();
        string jobName = nameof(LoadCookiesJob);

        ApplicationLogModelJar logJar = new ApplicationLogModelJar();
        logJar.JobName = jobName;
        logJar.JobId = jobId;
        logJar.LogAdded += LogJar_LogAdded;

        ApplicationLog jobStartInformationLog = ApplicationLogBusiness.CreateInformationLog($" LoadCookiesJob {start.ToString()} is started.", jobName, jobId, start);
        logJar.AddLog(new ApplicationLogModel(jobStartInformationLog));

        try
        {
            CrawlerHttpClientBase crawlerHttp = new PuppeterHttpClient(new HttpClients.ClientConfiguration()
            {
                Logger = _logger,
                LoggingJar = logJar
            });
            await crawlerHttp.ConfigureAsync();
            string html = await crawlerHttp.CrawlAsync("https://www.amazon.com.tr/");

            //PuppeterHttpClient puppeteerSharpClient = new PuppeterHttpClient(new HttpClients.ClientConfiguration()
            //{
            //    Logger = _logger,
            //    LoggingJar = logJar
            //});
            //await puppeteerSharpClient.ConfigureAsync();
            //string html = await puppeteerSharpClient.CrawlAsync("https://www.amazon.com.tr/");

            _logger.Log(LogLevel.Information, $"LoadCookiesJob Cookie Saved");
        }
        catch (PuppeteerCrawlCookieException ex)
        {
            ApplicationLog crawlCookieErrorLog =  ApplicationLogBusiness.CreateErrorLogFromException("PuppeteerCrawlCookieException is occured in LoadCookiesJob", jobName, jobId, ex, start, DateTime.Now);
            logJar.AddLog(new ApplicationLogModel(crawlCookieErrorLog));
        }
        catch (PuppeteerDirectoryAccessException ex)
        {
            ApplicationLog directoryAccessErrorLog =  ApplicationLogBusiness.CreateErrorLogFromException("PuppeteerDirectoryAccessException is occured in LoadCookiesJob", jobName, jobId, ex, start, DateTime.Now);
            logJar.AddLog(new ApplicationLogModel(directoryAccessErrorLog));
        }
        catch (PuppeteerDownloadException ex)
        {
            ApplicationLog downloadErrorLog = ApplicationLogBusiness.CreateErrorLogFromException("PuppeteerDownloadException is occured in LoadCookiesJob", jobName, jobId, ex, start, DateTime.Now);
            logJar.AddLog(new ApplicationLogModel(downloadErrorLog));
        }
        catch (PuppeteerExecutablePathException ex)
        {
            ApplicationLog executablePathErrorLog = ApplicationLogBusiness.CreateErrorLogFromException("PuppeteerExecutablePathException is occured in LoadCookiesJob", jobName, jobId, ex, start, DateTime.Now);
            logJar.AddLog(new ApplicationLogModel(executablePathErrorLog));
        }
        catch (Exception ex)
        {
            ApplicationLog errorLog = ApplicationLogBusiness.CreateErrorLogFromException("Exception is occured in LoadCookiesJob", jobName, jobId, ex, start, DateTime.Now);
            logJar.AddLog(new ApplicationLogModel(errorLog));
        }
        finally
        {
            DateTime finish = DateTime.Now;
            ApplicationLog jobEndInformationLog = ApplicationLogBusiness.CreateInformationLog($"LoadCookiesJob {finish.ToString()} is finished.", jobName, jobId, finish, finish - start);
            logJar.AddLog(new ApplicationLogModel(jobEndInformationLog));

            logJar.LogAdded -= LogJar_LogAdded;
        }
    }

    private void LogJar_LogAdded(object sender, ApplicationLogModelEventArgs e)
    {
        ApplicationLogModelJar jar = (sender as ApplicationLogModelJar)!;
        ApplicationLog appLog = e.ApplicationLogModel.AppLog;
        

    }
}