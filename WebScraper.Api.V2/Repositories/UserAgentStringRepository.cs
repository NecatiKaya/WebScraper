using Microsoft.EntityFrameworkCore;
using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Repositories;

public class UserAgentStringRepository : RepositoryBase
{
    public UserAgentStringRepository(WebScraperDbContext context) : base(context)
    {

    }

    public async Task AddUserAgentStringsAsync(List<UserAgentString> agents)
    {
        await _dbContext.UserAgentStrings.AddRangeAsync(agents.ToArray());
        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserAgentString> GetRandomUserAgentAsync()
    {
        UserAgentString? ua = await _dbContext.UserAgentStrings.AsNoTracking()
            .Where(x => x.Product == "Chrome" || x.Product == "Firefox" || x.Product == "Mozilla" || x.Product == "Edge")
            .OrderBy((x) => Guid.NewGuid()).FirstOrDefaultAsync();
        return ua!;
    }
}
