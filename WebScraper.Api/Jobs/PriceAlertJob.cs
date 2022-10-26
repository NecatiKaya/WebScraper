using Quartz;
using WebScraper.Api.Business;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;

namespace WebScraper.Api.Jobs;

public class PriceAlertJob : IJob
{
    private readonly ILogger<CrawlJob> _logger;

    private readonly WebScraperDbContext _dbContext;

    private readonly IMailSender _mailSender;

    public PriceAlertJob(ILogger<CrawlJob> logger, WebScraperDbContext dbContext, IMailSender mailSender)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mailSender = mailSender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime start = DateTime.Now;
        _logger.Log(LogLevel.Information, $" PriceAlertJob {start.ToString()} is started.");

        WebScraperBusiness business = new WebScraperBusiness(_dbContext, _mailSender);
        await business.SendPriceAlertEmail();

        DateTime finish = DateTime.Now;
        _logger.Log(LogLevel.Information, $" PriceAlertJob {finish.ToString()} is finished.");
    }
}
