using WebScraper.Api.Data.Models;

namespace WebScraper.Api.Business;

public class RepositoryBusiness
{
    public WebScraperDbContext DbContext { get; set; }

    public RepositoryBusiness(WebScraperDbContext ctx)
	{
        DbContext = ctx;
    }
}