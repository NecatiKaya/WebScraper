using Microsoft.AspNetCore.Mvc;
using WebScraper.Api.Business;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Dto;

namespace WebScraper.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    private readonly WebScraperDbContext _webScraperDbContext;

    private readonly IMailSender _mailSender;

    public EmailController(ILogger<ProductsController> logger, WebScraperDbContext webScraperDbContext, IMailSender mailSender)
    {
        _logger = logger;
        _webScraperDbContext = webScraperDbContext;
        _mailSender = mailSender;
    }

    [HttpPost("price-alert")]
    public async Task<IActionResult> SendPriceAlertEmail()
    {
        await new WebScraperBusiness(_webScraperDbContext, _mailSender).SendPriceAlertEmail();
        return Ok();
    }
}