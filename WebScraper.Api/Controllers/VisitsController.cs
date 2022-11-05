using Microsoft.AspNetCore.Mvc;
using WebScraper.Api.Business;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Dto;
using WebScraper.Api.Dto.ScraperVisit;

namespace WebScraper.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class VisitsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    private readonly WebScraperDbContext _webScraperDbContext;

    private readonly IMailSender _mailSender;

    private readonly RepositoryBusiness _repositoryBusiness;

    public VisitsController(ILogger<ProductsController> logger, WebScraperDbContext webScraperDbContext, IMailSender mailSender, RepositoryBusiness repositoryBusiness)
    {
        _logger = logger;
        _webScraperDbContext = webScraperDbContext;
        _mailSender = mailSender;
        _repositoryBusiness = repositoryBusiness;
    }

    [HttpPost()]
    public async Task<ActionResult<ServerResponse<GetScraperVisitDto>>> GetScraperVisits(ServerPagingRequest request)
    {
        ServerResponse<GetScraperVisitDto> visitsResponse = await new WebScraperBusiness(_webScraperDbContext, _mailSender, _repositoryBusiness).GetVisits(request);
        return Ok(visitsResponse);
    }
}