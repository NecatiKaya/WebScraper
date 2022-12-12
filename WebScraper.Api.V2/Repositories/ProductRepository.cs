using Microsoft.EntityFrameworkCore;
using WebScraper.Api.V2.Data.Dto;
using WebScraper.Api.V2.Data.Dto.Product;
using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Repositories;

public class ProductRepository : RepositoryBase
{
    public ProductRepository(WebScraperDbContext context) : base(context)
    {

    }

    public async Task<RepositoryResponseBase<Product>> GetAllProductsAsync(string sortKey, string sortOrder, int pageSize, int pageIndex)
    {
        IQueryable<Product> result = _dbContext.Products.AsNoTracking()
            .Skip((pageIndex * pageSize))
            .Take(pageSize);

        if (sortKey.ToLower() == "id")
        {
            if (sortOrder.ToLower() == "asc")
            {
                result = result.OrderBy(p => p.Id);
            }
            else
            {
                result = result.OrderByDescending(p => p.Id);
            }
        }

        if (sortKey.ToLower() == "name")
        {
            if (sortOrder.ToLower() == "asc")
            {
                result = result.OrderBy(p => p.Name);
            }
            else
            {
                result = result.OrderByDescending(p => p.Name);
            }
        }

        Product[] products = await result.ToArrayAsync();
        int count = await _dbContext.Products.AsNoTracking().CountAsync();

        RepositoryResponseBase<Product> repoResponse = new RepositoryResponseBase<Product>();
        repoResponse.Data = products;
        repoResponse.Count = count;
        return repoResponse;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        Product? _product = await (from product in _dbContext.Products.AsNoTracking()
                            where product.Id == id
                            select product).FirstOrDefaultAsync();

        return _product;
    }

    public async Task<RepositoryResponseBase<Product>> GetProductLikeByNameAsync(string name)
    {
        string loweredName = name.ToLower();
        Product[] result = await _dbContext.Products.AsNoTracking().Where(product => product.Name.ToLower().StartsWith(loweredName)).Take(50).ToArrayAsync();
        int count = await _dbContext.Products.AsNoTracking().Where(product => product.Name.ToLower().StartsWith(loweredName)).CountAsync();

        RepositoryResponseBase<Product> response = new RepositoryResponseBase<Product>();
        response.Data = result;
        response.Count = count;
        return response;
    }

    public async Task<RepositoryResponseBase<Product>> GetActiveProductsAsync()
    {
        Product[] result = await (from product in _dbContext.Products.AsNoTracking()
                                  where !product.IsDeleted
                                  select product).ToArrayAsync();

        int count = await _dbContext.Products.AsNoTracking().Where(eachProduct=> !eachProduct.IsDeleted).CountAsync();

        RepositoryResponseBase<Product> response = new RepositoryResponseBase<Product>();
        response.Data = result;
        response.Count = count;
        return response;
    }

    public async Task<Product> AddProductAsync(ProductAddDto productToAdd)
    {
        Product product = productToAdd.ToProduct();
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateProductAsync(ProductUpdateDto productToUpdate)
    {
        Product? product = await _dbContext.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == productToUpdate.Id);
        if (product is not null)
        {
            product.TrendyolUrl = productToUpdate.TrendyolUrl!;
            product.ASIN = productToUpdate.ASIN!;
            product.AmazonUrl = productToUpdate.AmazonUrl!;
            product.Barcode = productToUpdate.Barcode!;
            product.Name = productToUpdate.Name!;
            product.RequestedPriceDifferenceWithAmount = productToUpdate.RequestedPriceDifferenceWithAmount;
            product.RequestedPriceDifferenceWithPercentage = productToUpdate.RequestedPriceDifferenceWithPercentage;
            await _dbContext.SaveChangesAsync();
        }

        return product;
    }

    public async Task<Product?> DeleteProductAsync(int productId)
    {
        Product? product = await _dbContext.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == productId);
        if (product is not null)
        {
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }

        return product;
    }

    public async Task<int> GetProductCountAsync(bool onlyActiveCount)
    {
        int count = await _dbContext.Products.AsTracking().CountAsync((product) => (onlyActiveCount && !product.IsDeleted) || !onlyActiveCount);
        return count;
    }

    public async Task AddMultipleProducts(Product[] products)
    {
        await _dbContext.Products.AddRangeAsync(products);
        await _dbContext.SaveChangesAsync();
    }
}