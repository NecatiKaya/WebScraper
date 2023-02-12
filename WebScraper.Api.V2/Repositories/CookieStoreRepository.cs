﻿using Microsoft.EntityFrameworkCore;
using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Repositories;

public class CookieStoreRepository : LogDbRepositoryBase
{
    public CookieStoreRepository(WebScraperLogDbContext context) : base(context)
    {

    }

    public async Task<CookieStore> SaveCookieAsync(CookieStore cookie)
    {
        _dbContext.CookieStores.Add(cookie);
        await _dbContext.SaveChangesAsync();
        return cookie;
    }

    public async Task SaveCookiesAsync(CookieStore[] cookies)
    {
        await _dbContext.CookieStores.AddRangeAsync(cookies);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string cookie, Websites website)
    {
        bool exists = await _dbContext.CookieStores.Where(cookie => !cookie.IsUsed && cookie.WebSite == website).AnyAsync();
        return exists;
    }

    public async Task<CookieStore?> GetNotUsedCookieAsync(Websites website)
    {
        CookieStore? _cookie = await _dbContext.CookieStores.Where(cookie => /*!cookie.IsUsed &&*/ cookie.WebSite == website).OrderBy((x) => Guid.NewGuid()).FirstOrDefaultAsync();
        if (_cookie != null)
        {
            _cookie.IsUsed = true;
            await _dbContext.SaveChangesAsync();
        }

        return _cookie;
    }
}
