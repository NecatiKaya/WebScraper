using Microsoft.EntityFrameworkCore;
using WebScraper.Api.V2.Data.Dto;
using WebScraper.Api.V2.Data.Dto.ScraperVisit;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Extenisons;

namespace WebScraper.Api.V2.Repositories;
public class ScraperVisitRepository : RepositoryBase
{
    public ScraperVisitRepository(WebScraperDbContext context) : base(context)
    {

    }

    public async Task<RepositoryResponseBase<GetScraperVisitDto>> GetVisitsAsync(string sortKey, string sortOrder, int pageSize, int pageIndex, List<KeyValuePair<string, object>>? parameters = null)
    {
        DateTime? _startDate = null;
        DateTime? _endDate = null;
        int? productId = null;

        if (parameters?.GetParameterValue("startDate") != null)
        {
            _startDate = Convert.ToDateTime(parameters?.GetParameterValue("startDate")).Date;
        }

        if (parameters?.GetParameterValue("endDate") != null)
        {
            _endDate = Convert.ToDateTime(parameters?.GetParameterValue("endDate")).Date.AddHours(24).AddTicks(-1);
        }

        if (parameters?.GetParameterValue("productId") != null)
        {
            productId = Convert.ToInt32(parameters?.GetParameterValue("productId"));
        }

        IQueryable<GetScraperVisitDto> originalQuery = (from visit in _dbContext.ScraperVisits.AsNoTracking()
                                                        join product in _dbContext.Products.AsNoTracking() on visit.ProductId equals product.Id
                                                        where
                                                            (visit.ProductId == productId || productId == null) &&
                                                            (visit.VisitDate.Date >= _startDate || _startDate == null) &&
                                                            (visit.VisitDate.Date <= _endDate || _endDate == null)
                                                        select new GetScraperVisitDto
                                                        {
                                                            VisitId = visit.Id,
                                                            ProductId = visit.ProductId,
                                                            ProductName = product.Name,
                                                            VisitDate = visit.VisitDate,
                                                            AmazonPreviousPrice = visit.AmazonPreviousPrice,
                                                            AmazonCurrentPrice = visit.AmazonCurrentPrice,
                                                            AmazonCurrentDiscountAsAmount = visit.AmazonCurrentDiscountAsAmount,
                                                            AmazonCurrentDiscountAsPercentage = visit.AmazonCurrentDiscountAsPercentage,
                                                            TrendyolPreviousPrice = visit.TrendyolPreviousPrice,
                                                            TrendyolCurrentPrice = visit.TrendyolCurrentPrice,
                                                            TrendyolCurrentDiscountAsAmount = visit.TrendyolCurrentDiscountAsAmount,
                                                            TrendyolCurrentDiscountAsPercentage = visit.TrendyolCurrentDiscountAsPercentage,
                                                            CalculatedPriceDifferenceAsAmount = visit.CalculatedPriceDifferenceAsAmount,
                                                            CalculatedPriceDifferenceAsPercentage = visit.CalculatedPriceDifferenceAsPercentage,
                                                            RequestedPriceDifferenceAsAmount = visit.RequestedPriceDifferenceAsAmount,
                                                            RequestedPriceDifferenceAsPercentage = visit.RequestedPriceDifferenceAsPercentage,
                                                            NeedToNotify = visit.NeedToNotify,
                                                            Notified = visit.Notified,
                                                            LogId   = visit.LogId,
                                                            PriceNotFoundReason = visit.PriceNotFoundReason,
                                                            UsedCookieValue = visit.UsedCookieValue,
                                                            UsedUserAgentValue = visit.UsedUserAgentValue
                                                        });

        if (sortKey == "visitDate")
        {
            if (sortOrder == "asc")
            {
                originalQuery = originalQuery.OrderBy(v => v.VisitDate);
            }
            else
            {
                originalQuery = originalQuery.OrderByDescending(v => v.VisitDate);
            }
        }

        IQueryable<GetScraperVisitDto> pagedQuery = originalQuery
                     .Skip((pageIndex * pageSize))
                    .Take(pageSize);

        RepositoryResponseBase<GetScraperVisitDto> responseBase = new RepositoryResponseBase<GetScraperVisitDto>();
        responseBase.Data = await pagedQuery.ToArrayAsync();
        responseBase.Count = await originalQuery.CountAsync();
        return responseBase;
    }

    public async Task AddVisitsAsync(ScraperVisit[] visits)
    {    
        await _dbContext.ScraperVisits.AddRangeAsync(visits);
        await _dbContext.SaveChangesAsync();
    }

    public async Task VisitsNotifiedAsync(int[] visitIds)
    {
        if (visitIds == null || visitIds.Length == 0)
        {
            return;
        }
        List<ScraperVisit> resultItems = await (from visit in _dbContext.ScraperVisits
                                                where visitIds.Contains(visit.Id)
                                                select visit).ToListAsync();

        resultItems.ForEach(v =>
        {
            v.Notified = true;
        });

        await _dbContext.SaveChangesAsync();
    }
}
