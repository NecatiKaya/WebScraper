using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Repositories;

public abstract class RepositoryBase
{
	protected readonly WebScraperDbContext _dbContext;

	public RepositoryBase(WebScraperDbContext context)
	{
		_dbContext= context;
	}
}
