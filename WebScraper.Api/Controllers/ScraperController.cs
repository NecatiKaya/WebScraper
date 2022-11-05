using Microsoft.AspNetCore.Mvc;
using WebScraper.Api.Business;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;

namespace WebScraper.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ScraperController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    private readonly WebScraperDbContext _webScraperDbContext;

    private readonly IMailSender _mailSender;

    private readonly RepositoryBusiness _repositoryBusiness;

    public ScraperController(ILogger<ProductsController> logger, WebScraperDbContext webScraperDbContext, IMailSender mailSender, RepositoryBusiness repositoryBusiness)
    {
        _logger = logger;
        _webScraperDbContext = webScraperDbContext;
        _mailSender = mailSender;
        _repositoryBusiness = repositoryBusiness;
    }

    [HttpPost("crawl")]
    public async Task<IActionResult> Crawl()
    {
        WebScraperBusiness business = new WebScraperBusiness(_webScraperDbContext, _mailSender, _repositoryBusiness);
        await business.CrawlAllProductsV3();
        await business.SendPriceAlertEmail();
        return Ok();
    }
}