using WebScraper.Api.Business.Parsers;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Dto;
using WebScraper.Api.HttpClients;
using WebScraper.Api.Utilities;

namespace WebScraper.Api.Business;

public class CrawlingBusiness
{
    public readonly WebScraperDbContext DbContext;

    public CrawlingBusiness(WebScraperDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public async Task<ScraperVisit> CrawlProduct(Product product)
    {
        //string? trendyolHtml = await VisitUrl(product.TrendyolUrl);
        //string? amazonHtml = await VisitUrl(product.AmazonUrl);

        FlurlHttpClient client = new FlurlHttpClient(DbContext);
        string? trendyolHtml = await client.DownloadPageAsStringAsAsync(product.TrendyolUrl, product.Id);
        string? amazonHtml = await client.DownloadPageAsStringAsAsync(product.AmazonUrl, product.Id);

        ProductPriceInformation? trendyolPriceInformation = GetPriceFromHtml(trendyolHtml, Websites.Trendyol);
        ProductPriceInformation? amazonPriceInformation = GetPriceFromHtml(amazonHtml, Websites.Amazon);

        ScraperVisit visit = MappingHelper.GetScraperVisit(product, trendyolPriceInformation, amazonPriceInformation);
        return visit;
    }

    public async Task<List<ScraperVisit>> CrawlProducts(List<Product> products)
    {
        List<ScraperVisit> visits = new List<ScraperVisit>(products.Count);
        foreach (Product eachProduct in products)
        {
            string? trendyolHtml = await VisitUrl(eachProduct.TrendyolUrl);
            string? amazonHtml = await VisitUrl(eachProduct.AmazonUrl);

            ProductPriceInformation? trendyolPriceInformation = GetPriceFromHtml(trendyolHtml, Websites.Trendyol);
            ProductPriceInformation? amazonPriceInformation = GetPriceFromHtml(amazonHtml, Websites.Amazon);

            ScraperVisit visit = MappingHelper.GetScraperVisit(eachProduct, trendyolPriceInformation, amazonPriceInformation);
            visits.Add(visit);
        }

        return visits;
    }

    private async Task<string?> VisitUrl(string url)
    {
        string? html = await HtmlInfo.GetHtml(url);
        return html;
    }

    private ProductPriceInformation? GetPriceFromHtml(string? html, Websites site)
    {
        if (html == null)
        {
            return null;
        }
        IHtmlParser parser;
        if (site == Websites.Amazon)
        {
            parser = new AmazonParser(html);
        }
        else
        {
            parser = new TrendyolParser(html);
        }

        ProductPriceInformation? productPriceInformation = parser.Parse();
        return productPriceInformation;
    }
}