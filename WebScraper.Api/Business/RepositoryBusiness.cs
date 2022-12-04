﻿using Microsoft.EntityFrameworkCore;
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

    public async Task AddUserAgentStrings(List<UserAgentString> agents)
    {
        await DbContext.UserAgentStrings.AddRangeAsync(agents.ToArray());
        await DbContext.SaveChangesAsync();
    }

    public async Task<UserAgentString> GetRandomUserAgent()
    {
        UserAgentString? ua = await DbContext.UserAgentStrings.AsNoTracking()
            .Where(x=> x.Product == "Chrome" || x.Product == "Firefox" || x.Product == "Mozilla" || x.Product == "Edge")
            .OrderBy((x) => Guid.NewGuid()).FirstOrDefaultAsync();
        return ua!;
    }

    public async Task<CookieStore> SaveCookie(CookieStore cookie)
    {
        DbContext.CookieStores.Add(cookie);
        await DbContext.SaveChangesAsync();
        return cookie;
    }

    public async Task<CookieStore> GetNotUsedCookie(Websites website)
    {
        CookieStore? _cookie = await DbContext.CookieStores.Where(cookie => !cookie.IsUsed && cookie.WebSite == website).FirstOrDefaultAsync();
        if (_cookie != null)
        {
            _cookie.IsUsed = true;
            await DbContext.SaveChangesAsync();
        }
        
        return _cookie;
    }
}