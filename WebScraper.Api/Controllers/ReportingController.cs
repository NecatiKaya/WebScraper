using Microsoft.AspNetCore.Mvc;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Dto.ScraperVisit;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ReportingController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    private readonly WebScraperDbContext _webScraperDbContext;

    private readonly IMailSender _mailSender;

    public ReportingController(ILogger<ProductsController> logger, WebScraperDbContext webScraperDbContext, IMailSender mailSender)
    {
        _logger = logger;
        _webScraperDbContext = webScraperDbContext;
        _mailSender = mailSender;
    }

    //public async Task<ActionResult<ServerResponse<GetScraperVisitDto>>> ProductInfo(ServerPagingRequest request)
    //{

    //}
}