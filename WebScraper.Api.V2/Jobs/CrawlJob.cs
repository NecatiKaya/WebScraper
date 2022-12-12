using Flurl.Http;
using Quartz;
using WebScraper.Api.V2.Business.Email;
using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Jobs;

public class CrawlJob : IJob
{
    private readonly ILogger<CrawlJob> _logger;

    private readonly WebScraperDbContext _dbContext;

    private readonly IMailSender _mailSender;

    public CrawlJob(ILogger<CrawlJob> logger, WebScraperDbContext dbContext, IMailSender mailSender)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mailSender = mailSender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime start = DateTime.Now;
        _logger.Log(LogLevel.Information, $" CrawlJob {start.ToString()} is started.");

        Guid jobId = Guid.NewGuid();
        //await LogHelper.SaveInformationLog(null, $" CrawlJob {start.ToString()} is started.", jobId.ToString());
        try
        {
            WebScraperBusiness business = new WebScraperBusiness(_dbContext, _mailSender, _repositoryBusiness);
            await business.CrawlAllProductsV3();
            await business.SendPriceAlertEmail();
        }
        catch (Exception ex)
        {
            if (ex is FlurlHttpException)
            {
                (ex as FlurlHttpException)?.Call.LogErrorCallAsync();
            }
            //await LogHelper.SaveErrorLog(ex, null, null, "Job Ex", null, null, -2000);
            _logger.Log(LogLevel.Information, $" CrawlJob error occured. Stack Trace: {ex.StackTrace}. Message: {ex.Message}");
        }

        DateTime finish = DateTime.Now;
        _logger.Log(LogLevel.Information, $" CrawlJob {finish.ToString()} is finished.");

        //await LogHelper.SaveInformationLog(null, $" CrawlJob {finish.ToString()} is finished.", jobId.ToString());
    }
}