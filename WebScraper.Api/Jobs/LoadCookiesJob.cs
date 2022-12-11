using Quartz;
using System.Net;
using WebScraper.Api.Business;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;
using WebScraper.Api.HttpClients.PuppeteerClient;
using WebScraper.Api.Utilities;

namespace WebScraper.Api.Jobs;

public class LoadCookiesJob : IJob
{
    private readonly ILogger<CrawlJob> _logger;

    private readonly WebScraperDbContext _dbContext;

    private readonly IMailSender _mailSender;

    private readonly RepositoryBusiness _repositoryBusiness;

    public LoadCookiesJob(ILogger<CrawlJob> logger, WebScraperDbContext dbContext, IMailSender mailSender, RepositoryBusiness repositoryBusiness)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mailSender = mailSender;
        _repositoryBusiness = repositoryBusiness;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime start = DateTime.Now;
        _logger.Log(LogLevel.Information, $" LoadCookiesJob {start.ToString()} is started.");

        Guid jobId = Guid.NewGuid();
        await LogHelper.SaveInformationLog(null, $" LoadCookiesJob {start.ToString()} is started.", jobId.ToString());
        try
        {
            PuppeteerSharpClient puppeteerSharpClient = new PuppeteerSharpClient();
            await puppeteerSharpClient.Prepare();
            string[] cookies = await puppeteerSharpClient.GetNewCookies("https://www.amazon.com.tr/");
            _logger.Log(LogLevel.Information, $"LoadCookiesJob Cookies are Loaded");
            if (cookies?.Length > 0)
            {
                CookieStore[] store = cookies.Select(eachCookie => new CookieStore() { CookieValue = eachCookie, CreateDate = DateTime.Now, IsUsed = false, WebSite = Websites.Amazon }).ToArray();
                await _repositoryBusiness.SaveCookies(store);
            }
            
            _logger.Log(LogLevel.Information, $"LoadCookiesJob Cookie Saved");
        }
        catch (Exception ex)
        {
            await LogHelper.SaveErrorLog(ex, null, null, "LoadCookiesJob Job Ex", null, null, -7010);
            _logger.Log(LogLevel.Information, $" LoadCookiesJob error occured. Stack Trace: {ex.StackTrace}. Message: {ex.Message}");
        }

        DateTime finish = DateTime.Now;
        _logger.Log(LogLevel.Information, $" LoadCookiesJob {finish.ToString()} is finished.");

        await LogHelper.SaveInformationLog(null, $" LoadCookiesJob {finish.ToString()} is finished.", jobId.ToString());
    }
}