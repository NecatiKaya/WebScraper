using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebScraper.Api.V2.Business.Email;
using WebScraper.Api.V2.Data.Dto;
using WebScraper.Api.V2.Data.Dto.Product;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    private readonly WebScraperDbContext _webScraperDbContext;

    private readonly IMailSender _mailSender;

    private readonly ProductRepository _productRepository;

    public ProductsController(ILogger<ProductsController> logger, WebScraperDbContext webScraperDbContext, IMailSender mailSender, ProductRepository productRepository)
    {
        _logger = logger;
        _webScraperDbContext = webScraperDbContext;
        _mailSender = mailSender;
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ServerResponse<Product>>> Get([FromQuery] string sortKey, [FromQuery] string sortDirection, [FromQuery] int pageSize = 50, [FromQuery] int pageIndex = 0)
    {
        RepositoryResponseBase<Product> products = await _productRepository.GetAllProductsAsync(sortKey, sortDirection, pageSize, pageIndex);
        ServerResponse<Product> response = new ServerResponse<Product>()
        {
            Data = products.Data,
            TotalRowCount = products.Count
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerResponse<Product>>> GetById([FromRoute] int id)
    {
        Product? product = await _productRepository.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound(id);
        }

        ServerResponse<Product> response = new ServerResponse<Product>()
        {
            Data = new Product[] { product },
            TotalRowCount = 1
        };
        return Ok(response);
    }


    [HttpGet("like-search")]
    public async Task<ActionResult<ServerResponse<Product>>> GetByNameLike(string? name = "")
    {
        RepositoryResponseBase<Product> productResponse = await _productRepository.GetProductLikeByNameAsync(name ?? string.Empty);
        ServerResponse<Product> response = new ServerResponse<Product>()
        {
            Data = productResponse.Data,
            TotalRowCount = productResponse.Count
        };
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ServerResponse<Product>>> Add(ProductAddDto productToAdd)
    {
        Product product = await _productRepository.AddProductAsync(productToAdd);
        ServerResponse<Product> response = new ServerResponse<Product>();
        response.Data = new List<Product>() { product };
        response.TotalRowCount = 1;
        return Ok(response);
    }

    [HttpPatch]
    public async Task<ActionResult<ServerResponse<Product>>> Update(ProductUpdateDto productToUpdate)
    {
        Product? product = await _productRepository.UpdateProductAsync(productToUpdate);
        ServerResponse<Product?> response = new ServerResponse<Product?>();
        response.Data = new List<Product?>() { product };
        response.TotalRowCount = 1;
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ServerResponse<Product>>> Delete([FromRoute] int id)
    {
        Product? product = await _productRepository.DeleteProductAsync(id);
        ServerResponse<Product?> response = new ServerResponse<Product?>();
        response.Data = new List<Product?>() { product };
        response.TotalRowCount = 1;
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
            await UploadProductFile(productFile);
        }
        catch (Exception ex)
        {
            serverResponse.IsSuccess = false;
            serverResponse.Data = new List<string>() { ex.Message };
        }

        return Ok(serverResponse);
    }

    private async Task UploadProductFile(IFormFile file)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            NewLine = Environment.NewLine,
            Delimiter = ";"
        };

        using (StreamReader reader = new StreamReader(file.OpenReadStream()))
        {
            using (CsvReader csv = new CsvReader(reader, config))
            {
                List<ProductFileDefinitionDto> records = csv.GetRecords<ProductFileDefinitionDto>().ToList();
                List<ProductFileDefinitionDto> validRecords = records.Where(eactRecord => eactRecord.IsValid()).ToList();

                List<Product> productItems = validRecords.Select(eachValidItem =>
                new Product(eachValidItem.Name!, eachValidItem.Barcode!, eachValidItem.Asin!, eachValidItem.TrendyolLink!, eachValidItem.AmazonLink!)
                {
                    IsDeleted = false,
                    RequestedPriceDifferenceWithPercentage = eachValidItem.Percentage!
                }).ToList();

                await _productRepository.AddMultipleProducts(productItems.ToArray());
            }
        }
    }
}