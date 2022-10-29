using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using WebScraper.Api.Business;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Dto;
using WebScraper.Api.Dto.Product;

namespace WebScraper.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    private readonly WebScraperDbContext _webScraperDbContext;

    private readonly IMailSender _mailSender;

    public ProductsController(ILogger<ProductsController> logger, WebScraperDbContext webScraperDbContext, IMailSender mailSender)
    {
        _logger = logger;
        _webScraperDbContext = webScraperDbContext;
        _mailSender = mailSender;
    }

    [HttpGet]
    public async Task<ActionResult<ServerResponse<Product>>> Get([FromQuery] string sortKey, [FromQuery] string sortDirection, [FromQuery] int pageSize = 50, [FromQuery] int pageIndex = 0)
    {
        ServerPagingRequest request = new ServerPagingRequest()
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            SortDirection = sortDirection,
            SortKey = sortKey,
        };
        ServerResponse<Product> products = await new WebScraperBusiness(_webScraperDbContext, _mailSender).GetAllProducts(request);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerResponse<Product>>> GetById([FromRoute] int id)
    {
        ServerResponse<Product> products = await new WebScraperBusiness(_webScraperDbContext, _mailSender).GetProductById(id);
        return Ok(products);
    }


    [HttpGet("like-search")]
    public async Task<ActionResult<ServerResponse<Product>>> GetByNameLike(string? name = "")
    {
        ServerResponse<Product> products = await new WebScraperBusiness(_webScraperDbContext, _mailSender).GetProductLikeByName(name ?? String.Empty);
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<ServerResponse<Product>>> Add(ProductAddDto productToAdd)
    {
        Product product = await new WebScraperBusiness(_webScraperDbContext, _mailSender).AddProduct(productToAdd);
        ServerResponse<Product> response = new ServerResponse<Product>();
        response.Data = new List<Product>() { product };
        return Ok(response);
    }

    [HttpPatch]
    public async Task<ActionResult<ServerResponse<Product>>> Update(ProductUpdateDto productToUpdate)
    {
        Product product = await new WebScraperBusiness(_webScraperDbContext, _mailSender).UpdateProduct(productToUpdate);
        ServerResponse<Product> response = new ServerResponse<Product>();
        response.Data = new List<Product>() { product };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ServerResponse<Product>>> Delete([FromRoute] int id)
    {
        Product product = await new WebScraperBusiness(_webScraperDbContext, _mailSender).DeleteProduct(id);
        ServerResponse<Product> response = new ServerResponse<Product>();
        response.Data = new List<Product>() { product };
        return Ok(response);
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ServerResponse<string>>> UploadProductFile()
    {
        ServerResponse<string> serverResponse = new ServerResponse<string>();
        if (Request.Form is null || Request.Form.Files is null || Request.Form.Files.Count == 0)
        {
            serverResponse.IsSuccess = false;
            serverResponse.Data = new List<string>() { "File is absent." };
            return BadRequest(serverResponse);
        }

        try
        {
            serverResponse.IsSuccess = true;
            IFormFile productFile = Request.Form.Files[0];
            await new WebScraperBusiness(_webScraperDbContext, _mailSender).UploadProductFile(productFile);
        }
        catch (Exception ex)
        {
            serverResponse.IsSuccess = false;
            serverResponse.Data = new List<string>() { ex.Message };
        }

        return Ok(serverResponse);
    }

    [HttpPost("crawl")]
    public async Task<IActionResult> Crawl()
    {
        await new WebScraperBusiness(_webScraperDbContext, _mailSender).CrawlAllProductsV3();
        return Ok();
    }
}