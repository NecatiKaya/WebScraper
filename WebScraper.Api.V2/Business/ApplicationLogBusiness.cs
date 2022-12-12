using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Business;

public class ApplicationLogBusiness
{
	private readonly ApplicationLogRepository _applicationLogRepository;

    public ApplicationLogBusiness(ApplicationLogRepository applicationLogRepository)
    {
        _applicationLogRepository = applicationLogRepository;
    }

	public async Task AddErrorAsync(ApplicationLog log)
	{
		await _applicationLogRepository.AddAsync(log);
	}
}