using Microsoft.AspNetCore.Mvc;
using WebScraper.Api.V2.Business.Email;
using WebScraper.Api.V2.Data.Dto;
using WebScraper.Api.V2.Data.Dto.ScraperVisit;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Controllers;

[ApiController]
[Route("[controller]")]
public class VisitsController : ControllerBase
{
    private readonly ILogger<VisitsController> _logger;

    private readonly WebScraperDbContext _webScraperDbContext;

    private readonly IMailSender _mailSender;

    private readonly ScraperVisitRepository _scraperVisitRepository;

    public VisitsController(ILogger<VisitsController> logger, WebScraperDbContext webScraperDbContext, IMailSender mailSender, ScraperVisitRepository scraperVisitRepository)
    {
        _logger = logger;
        _webScraperDbContext = webScraperDbContext;
        _mailSender = mailSender;
        _scraperVisitRepository = scraperVisitRepository;
    }

    [HttpPost()]
    public async Task<ActionResult<ServerResponse<GetScraperVisitDto>>> GetScraperVisits(ServerPagingRequest request)
    {
        RepositoryResponseBase<GetScraperVisitDto> visitsResponse = await _scraperVisitRepository.GetVisitsAsync(request.SortKey, request.SortDirection, request.PageSize, request.PageIndex);
        ServerResponse<GetScraperVisitDto> response = new ServerResponse<GetScraperVisitDto>() 
        { 
            Data = visitsResponse.Data,
            TotalRowCount = visitsResponse.Count
        };
        return Ok(response);
    }
}