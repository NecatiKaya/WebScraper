using Microsoft.EntityFrameworkCore;
using WebScraper.Api.Data.Models;

namespace WebScraper.Api.Business;

public class RepositoryBusiness
{
    public WebScraperDbContext DbContext { get; set; }

    public RepositoryBusiness(WebScraperDbContext ctx)
	{
        DbContext = ctx;
    }

    public async Task VisitsNotified(int[] visitIds)
    {
        if (visitIds == null || visitIds.Length == 0)
        {
            return;
        }
        List<ScraperVisit> resultItems = await (from visit in DbContext.ScraperVisits
                                          where visitIds.Contains(visit.Id)
                                          select visit).ToListAsync();

        resultItems.ForEach(v =>
        {
            v.Notified = true;
        });

        await DbContext.SaveChangesAsync();
    }
}