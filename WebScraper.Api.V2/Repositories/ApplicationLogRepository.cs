using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Repositories;

public class ApplicationLogRepository : LogDbRepositoryBase
{
    public ApplicationLogRepository(WebScraperLogDbContext context) : base(context)
    {

    }

    public async Task<ApplicationLog> AddAsync(ApplicationLog applicationLog)
    {
        await _dbContext.ApplicationLogs.AddAsync(applicationLog);
        _dbContext.SaveChanges();
        return applicationLog;
    }
}
