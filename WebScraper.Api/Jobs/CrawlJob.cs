using Quartz;
using WebScraper.Api.Business;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;

namespace WebScraper.Api.Jobs;

public class CrawlJob : IJob
{
    private readonly ILogger<CrawlJob> _logger;

    private readonly WebScraperDbContext _dbContext;

    private readonly IMailSender _mailSender;

    private readonly RepositoryBusiness _repositoryBusiness;

    public CrawlJob(ILogger<CrawlJob> logger, WebScraperDbContext dbContext, IMailSender mailSender, RepositoryBusiness repositoryBusiness)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mailSender = mailSender;
        _repositoryBusiness = repositoryBusiness;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime start = DateTime.Now;
        _logger.Log(LogLevel.Information, $" CrawlJob {start.ToString()} is started.");

        WebScraperBusiness business = new WebScraperBusiness(_dbContext, _mailSender, _repositoryBusiness);
        await business.CrawlAllProductsV3();
        //await business.CrawlAllProductsV2();

        DateTime finish = DateTime.Now;
        _logger.Log(LogLevel.Information, $" CrawlJob {finish.ToString()} is finished.");
    }
}