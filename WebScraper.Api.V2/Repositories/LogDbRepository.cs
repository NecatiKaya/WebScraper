using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Repositories;

public abstract class LogDbRepositoryBase
{
    protected readonly WebScraperLogDbContext _dbContext;

    public LogDbRepositoryBase(WebScraperLogDbContext context)
    {
        _dbContext = context;
    }
}