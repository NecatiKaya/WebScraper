using Microsoft.AspNetCore.Mvc;
using WebScraper.Api.Business;
using WebScraper.Api.Business.UserAgent;
using WebScraper.Api.Data.Models;

namespace WebScraper.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UserAgentController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    private readonly RepositoryBusiness _repositoryBusiness;

    public UserAgentController(ILogger<ProductsController> logger, RepositoryBusiness repositoryBusiness)
	{
		_logger= logger;
        _repositoryBusiness = repositoryBusiness;
    }

    [HttpPost("download")]
    public async Task<IActionResult> DownloadAndParse()
    {
        List<UserAgentString>? agents = (await UserAgentManager.DownloadAllAgents())?.Select((ua) =>
        {
            return new UserAgentString()
            {
                Agent = ua.Agent,
                Id = ua.Id,
                Product = ua.Product,
                Version = ua.Version,
            };
        }).ToList();
        if (agents != null)
        {
            await _repositoryBusiness.AddUserAgentStrings(agents);
        }
        
        return Ok();
    }
}