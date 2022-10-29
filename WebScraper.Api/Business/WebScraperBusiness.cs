using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Business.Parsers;
using WebScraper.Api.Business.Product_Upload;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Dto;
using WebScraper.Api.Dto.Product;
using WebScraper.Api.Dto.ScraperVisit;

namespace WebScraper.Api.Business;

public class WebScraperBusiness
{
    public WebScraperDbContext DbContext { get; set; }

    private IMailSender MailSender { get; set; }

    public WebScraperBusiness(WebScraperDbContext ctx, IMailSender mailSender)
    {
        DbContext = ctx;
        MailSender = mailSender;
    }

    public async Task<ServerResponse<Product>> GetAllProducts(ServerPagingRequest request)
    {
        var result = DbContext.Products.AsNoTracking()
            .Skip((request.PageIndex * request.PageSize))
            .Take(request.PageSize);

        if (request.SortKey?.ToLower() == "id")
        {
            if (request.SortDirection.ToLower() == "asc")
            {
                result = result.OrderBy(p => p.Id);
            }
            else
            {
                result = result.OrderByDescending(p => p.Id);
            }
        }

        if (request.SortKey?.ToLower() == "name")
        {
            if (request.SortDirection.ToLower() == "asc")
            {
                result = result.OrderBy(p => p.Name);
            }
            else
            {
                result = result.OrderByDescending(p => p.Name);
            }
        }

        List<Product> products = await result.ToListAsync();
        int count = await DbContext.Products.CountAsync();

        ServerResponse<Product> response = new ServerResponse<Product>();
        response.Data = products;
        response.TotalRowCount = count;
        response.IsSuccess = true;

        return response;
    }

    public async Task<ServerResponse<Product>> GetProductById(int id)
    {
        var result = await (from product in DbContext.Products.AsNoTracking()
                            where product.Id == id
                            select product).ToListAsync();

        ServerResponse<Product> response = new ServerResponse<Product>();
        response.Data = result;
        response.TotalRowCount = result.Count;
        response.IsSuccess = true;

        return response;
    }

    public async Task<ServerResponse<Product>> GetProductLikeByName(string name)
    {
        string loweredName = name.ToLower();
        var result = await DbContext.Products.AsNoTracking().Where(product => product.Name.ToLower().StartsWith(loweredName)).Take(50).ToListAsync();

        ServerResponse<Product> response = new ServerResponse<Product>();
        response.Data = result;
        response.TotalRowCount = result.Count;
        response.IsSuccess = true;

        return response;
    }

    public async Task<ServerResponse<Product>> GetActiveProducts()
    {
        var query = (from product in DbContext.Products.AsNoTracking()
                     where !product.IsDeleted
                     select product);
        ServerResponse<Product> response = new ServerResponse<Product>();
        response.Data = await query.ToListAsync();
        response.IsSuccess = true;
        response.TotalRowCount = response.Data.Count();
        return response;
        //var query = (from product in DbContext.Products.AsNoTracking()
        //             where !product.IsDeleted
        //             select product)
        //                .Skip((request.PageIndex * request.PageSize))
        //                .Take(request.PageSize). OrderBy(p=> p.Id);

        //if (request.SortKey?.ToLower() == "name")
        //{
        //    if (request.SortDirection.ToLower() == "asc")
        //    {
        //        query = query.OrderBy(p => p.Name);
        //    }
        //    else
        //    {
        //        query = query.OrderByDescending(p => p.Name);
        //    }
        //}

        //ServerResponse<Product> response = new ServerResponse<Product>();
        //response.Data = await query.ToListAsync();
        //response.IsSuccess = true;
        //response.TotalRowCount = await (from product in DbContext.Products.AsNoTracking()
        //                                where !product.IsDeleted
        //                                select product).CountAsync();
        //return response;
    }

    public async Task<Product> AddProduct(ProductAddDto productToAdd)
    {
        Product product = productToAdd.ToProduct();
        DbContext.Products.Add(product);
        await DbContext.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateProduct(ProductUpdateDto productToUpdate)
    {
        Product? product = await DbContext.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == productToUpdate.Id);
        if (product is not null)
        {
            product.TrendyolUrl = productToUpdate.TrendyolUrl!;
            product.ASIN = productToUpdate.ASIN!;
            product.AmazonUrl = productToUpdate.AmazonUrl!;
            product.Barcode = productToUpdate.Barcode!;
            product.Name = productToUpdate.Name!;
            product.RequestedPriceDifferenceWithAmount = productToUpdate.RequestedPriceDifferenceWithAmount;
            product.RequestedPriceDifferenceWithPercentage = productToUpdate.RequestedPriceDifferenceWithPercentage;

            await DbContext.SaveChangesAsync();
        }

        return product;
    }

    public async Task<Product?> DeleteProduct(int productId)
    {
        Product? product = await DbContext.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == productId);
        if (product is not null)
        {
            DbContext.Products.Remove(product);
            await DbContext.SaveChangesAsync();
        }

        return product;
    }

    public async Task<int> GetProductCount(bool onlyActiveCount)
    {
        int count = await DbContext.Products.AsTracking().CountAsync((product) => (onlyActiveCount && !product.IsDeleted) || !onlyActiveCount);
        return count;
    }

    public async Task<ServerResponse<GetScraperVisitDto>> GetVisits(ServerPagingRequest request)
    {
        DateTime? _startDate = null;
        DateTime? _endDate = null;
        int? productId = null;

        if (request.GetParamValue("startDate") != null)
        {
            _startDate = Convert.ToDateTime(request.GetParamValue("startDate")).Date;
        }

        if (request.GetParamValue("endDate") != null)
        {
            _endDate = Convert.ToDateTime(request.GetParamValue("endDate")).Date.AddHours(24).AddTicks(-1);
        }

        if (request.GetParamValue("productId") != null)
        {
            productId = Convert.ToInt32(request.GetParamValue("productId"));
        }

        IQueryable<GetScraperVisitDto> originalQuery = (from visit in DbContext.ScraperVisits.AsNoTracking()
                             join product in DbContext.Products.AsNoTracking() on visit.ProductId equals product.Id
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
                                 Notified = visit.Notified
                             });

        if (request.SortKey == "visitDate")
        {            
            if (request.SortDirection == "asc")
            {
                originalQuery = originalQuery.OrderBy(v => v.VisitDate);
            }
            else
            {
                originalQuery = originalQuery.OrderByDescending(v => v.VisitDate);
            }
        }

        IQueryable<GetScraperVisitDto> pagedQuery = originalQuery
                     .Skip((request.PageIndex * request.PageSize))
                    .Take(request.PageSize);

        List<GetScraperVisitDto> result = await pagedQuery.ToListAsync();
        ServerResponse<GetScraperVisitDto> response = new ServerResponse<GetScraperVisitDto>();
        response.Data = result;
        response.TotalRowCount = await originalQuery.CountAsync();
        response.IsSuccess = true;
        return response;
    }

    public async Task CrawlAllProducts()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        List<Task<HtmlInfos>> tasks = new List<Task<HtmlInfos>>();
        ServerResponse<Product> activeProductsResponse = await GetActiveProducts();
        int batchSize = 100;
        int numberOfBatches = (int)Math.Ceiling((double)activeProductsResponse.TotalRowCount / batchSize);

        for (int i = 0; i < numberOfBatches; i++)
        {
            IEnumerable<Product>? productsPerTask = activeProductsResponse.Data?.Skip(i * batchSize).Take(batchSize);
            if (productsPerTask is null)
            {
                continue;
            }
            IEnumerable<Task<HtmlInfos>>? crawlTasks = productsPerTask.Select(_product => CrawlProduct(_product));
            if (crawlTasks is null)
            {
                continue;
            }
            tasks.AddRange(crawlTasks);
        }

        IEnumerable<HtmlInfos> allHtmls = (await Task.WhenAll(tasks));
        foreach (HtmlInfos eachHtmlInfos in allHtmls)
        {
            IHtmlParser trendyolParser = new TrendyolParser(eachHtmlInfos.TrendyolHtmlInfo?.Html!);
            IHtmlParser amazonParser = new AmazonParser(eachHtmlInfos.AmazonHtmlInfo?.Html!);

            ProductPriceInformation? trendyolPriceInformation = trendyolParser.Parse();
            ProductPriceInformation? amazonPriceInformation = amazonParser.Parse();

            ScraperVisit visit = new ScraperVisit()
            {
                //Product = eachHtmlInfos?.Product!,
                ProductId = eachHtmlInfos!.Product!.Id!,
                VisitDate = DateTime.Now,
                Notified = false
            };
            if (trendyolPriceInformation is not null)
            {
                visit.TrendyolCurrentDiscountAsAmount = trendyolPriceInformation.CurrentDiscountAsAmount;
                visit.TrendyolCurrentDiscountAsPercentage = trendyolPriceInformation.CurrentDiscountAsPercentage;
                visit.TrendyolCurrentPrice = trendyolPriceInformation.CurrentPrice;
                visit.TrendyolPreviousPrice = trendyolPriceInformation.PreviousPrice;
            }
            
            if (amazonPriceInformation is not null)
            {
                visit.AmazonCurrentDiscountAsAmount = amazonPriceInformation.CurrentDiscountAsAmount;
                visit.AmazonCurrentDiscountAsPercentage = amazonPriceInformation.CurrentDiscountAsPercentage;
                visit.AmazonCurrentPrice = amazonPriceInformation.CurrentPrice;
                visit.AmazonPreviousPrice = amazonPriceInformation.PreviousPrice;
            }
            
            if (trendyolPriceInformation is not null && amazonPriceInformation is not null)
            {
                decimal calculatedDifferenceWithPercentage = 0;
                decimal calculatedDifferenceWithAmount = 0;
                bool needToNotify = false;

                calculatedDifferenceWithAmount = trendyolPriceInformation.CurrentPrice - amazonPriceInformation.CurrentPrice;
                calculatedDifferenceWithPercentage = calculatedDifferenceWithAmount * 100 / trendyolPriceInformation.CurrentPrice;

                if (eachHtmlInfos.Product.RequestedPriceDifferenceWithAmount is not null)
                {
                    needToNotify = calculatedDifferenceWithAmount >= eachHtmlInfos.Product.RequestedPriceDifferenceWithAmount;
                }

                if (eachHtmlInfos.Product.RequestedPriceDifferenceWithPercentage is not null)
                {
                    needToNotify = calculatedDifferenceWithPercentage >= eachHtmlInfos.Product.RequestedPriceDifferenceWithPercentage;
                }
                
                visit.CalculatedPriceDifferenceAsPercentage = calculatedDifferenceWithPercentage;
                visit.CalculatedPriceDifferenceAsAmount = calculatedDifferenceWithAmount;
                visit.RequestedPriceDifferenceAsPercentage = eachHtmlInfos!.Product!.RequestedPriceDifferenceWithPercentage;
                visit.RequestedPriceDifferenceAsAmount = eachHtmlInfos!.Product!.RequestedPriceDifferenceWithAmount;
                visit.NeedToNotify = needToNotify;
            }

            await DbContext.ScraperVisits.AddAsync(visit);
            await DbContext.SaveChangesAsync();
        }

        stopwatch.Stop();
        Console.WriteLine("Elapsed.............");
        Console.WriteLine(stopwatch.Elapsed);
    }

    public async Task CrawlAllProductsV2()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        List<Task<HtmlInfos>> tasks = new List<Task<HtmlInfos>>();
        ServerResponse<Product> activeProductsResponse = await GetActiveProducts();
        int batchSize = 10;
        int numberOfBatches = (int)Math.Ceiling((double)activeProductsResponse.TotalRowCount / batchSize);

        for (int i = 0; i < numberOfBatches; i++)
        {
            IEnumerable<Product>? productsPerTask = activeProductsResponse.Data?.Skip(i * batchSize).Take(batchSize);
            if (productsPerTask is null)
            {
                continue;
            }
            
            IEnumerable<Task<HtmlInfos>>? crawlTasks = productsPerTask.Select(_product => CrawlProduct(_product));
            if (crawlTasks is null)
            {
                continue;
            }
            IEnumerable<HtmlInfos> allHtmls = (await Task.WhenAll(crawlTasks));
            List<ScraperVisit> visits = new List<ScraperVisit>();
            foreach (HtmlInfos eachHtmlInfos in allHtmls)
            {
                IHtmlParser trendyolParser = new TrendyolParser(eachHtmlInfos.TrendyolHtmlInfo?.Html!);
                IHtmlParser amazonParser = new AmazonParser(eachHtmlInfos.AmazonHtmlInfo?.Html!);

                ProductPriceInformation? trendyolPriceInformation = trendyolParser.Parse();
                ProductPriceInformation? amazonPriceInformation = amazonParser.Parse();

                ScraperVisit visit = new ScraperVisit()
                {
                    //Product = eachHtmlInfos?.Product!,
                    ProductId = eachHtmlInfos!.Product!.Id!,
                    VisitDate = DateTime.Now,
                    Notified = false
                };
                if (trendyolPriceInformation is not null)
                {
                    visit.TrendyolCurrentDiscountAsAmount = trendyolPriceInformation.CurrentDiscountAsAmount;
                    visit.TrendyolCurrentDiscountAsPercentage = trendyolPriceInformation.CurrentDiscountAsPercentage;
                    visit.TrendyolCurrentPrice = trendyolPriceInformation.CurrentPrice;
                    visit.TrendyolPreviousPrice = trendyolPriceInformation.PreviousPrice;
                }

                if (amazonPriceInformation is not null)
                {
                    visit.AmazonCurrentDiscountAsAmount = amazonPriceInformation.CurrentDiscountAsAmount;
                    visit.AmazonCurrentDiscountAsPercentage = amazonPriceInformation.CurrentDiscountAsPercentage;
                    visit.AmazonCurrentPrice = amazonPriceInformation.CurrentPrice;
                    visit.AmazonPreviousPrice = amazonPriceInformation.PreviousPrice;
                }

                if (trendyolPriceInformation is not null && amazonPriceInformation is not null)
                {
                    decimal calculatedDifferenceWithPercentage = 0;
                    decimal calculatedDifferenceWithAmount = 0;
                    bool needToNotify = false;

                    calculatedDifferenceWithAmount = trendyolPriceInformation.CurrentPrice - amazonPriceInformation.CurrentPrice;
                    calculatedDifferenceWithPercentage = calculatedDifferenceWithAmount * 100 / trendyolPriceInformation.CurrentPrice;

                    if (eachHtmlInfos.Product.RequestedPriceDifferenceWithAmount is not null)
                    {
                        needToNotify = calculatedDifferenceWithAmount >= eachHtmlInfos.Product.RequestedPriceDifferenceWithAmount;
                    }

                    if (eachHtmlInfos.Product.RequestedPriceDifferenceWithPercentage is not null)
                    {
                        needToNotify = calculatedDifferenceWithPercentage >= eachHtmlInfos.Product.RequestedPriceDifferenceWithPercentage;
                    }

                    visit.CalculatedPriceDifferenceAsPercentage = calculatedDifferenceWithPercentage;
                    visit.CalculatedPriceDifferenceAsAmount = calculatedDifferenceWithAmount;
                    visit.RequestedPriceDifferenceAsPercentage = eachHtmlInfos!.Product!.RequestedPriceDifferenceWithPercentage;
                    visit.RequestedPriceDifferenceAsAmount = eachHtmlInfos!.Product!.RequestedPriceDifferenceWithAmount;
                    visit.NeedToNotify = needToNotify;                   
                }
                visits.Add(visit);
            }
            await DbContext.ScraperVisits.AddRangeAsync(visits);
            await DbContext.SaveChangesAsync();
            visits.Clear();
        }

        stopwatch.Stop();
        Console.WriteLine("Elapsed.............");
        Console.WriteLine(stopwatch.Elapsed);
    }

    public async Task CrawlAllProductsV3()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        ServerResponse<Product> activeProductsResponse = await GetActiveProducts();
        if (activeProductsResponse == null || activeProductsResponse.Data == null || activeProductsResponse.Data.Count() == 0)
        {
            return;
        }

        int batchSize = 50;
        int numberOfBatches = (int)Math.Ceiling((double)activeProductsResponse.TotalRowCount / batchSize);
        CrawlingBusiness crawlingBusiness = new CrawlingBusiness(DbContext);

        for (int i = 0; i < numberOfBatches; i++)
        {
            List<Product> productsPerTask = activeProductsResponse.Data.Skip(i * batchSize).Take(batchSize).ToList();
            if (productsPerTask is null)
            {
                continue;
            }

            IEnumerable<Task<ScraperVisit>> tasksForEachProduct = productsPerTask.Select(eachProduct => crawlingBusiness.CrawlProduct(eachProduct));            
            ScraperVisit[] visits = await Task.WhenAll(tasksForEachProduct);
            await DbContext.ScraperVisits.AddRangeAsync(visits);
            await DbContext.SaveChangesAsync();
        }

        stopwatch.Stop();
        var a = stopwatch.Elapsed;
    }

    public async Task<HtmlInfos> CrawlProduct(Product product)
    {
        HtmlInfos htmls = new();
        htmls.TrendyolHtmlInfo = await HtmlInfo.GetHtmlInfo(product.TrendyolUrl, Websites.Trendyol);
        htmls.AmazonHtmlInfo = await HtmlInfo.GetHtmlInfo(product.AmazonUrl, Websites.Amazon);
        htmls.Product = product;
        return htmls;
    }

    public async Task UploadProductFile(IFormFile file)
    {
        if (file is null)
        {
           throw new ArgumentNullException(nameof(file));
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            NewLine = Environment.NewLine,
            Delimiter = ";"
        };

        using (StreamReader reader = new StreamReader(file.OpenReadStream()))
        {    
            using (var csv = new CsvReader(reader, config))
            {
                List<ProductFileDefinition> records = csv.GetRecords<ProductFileDefinition>().ToList();
                List<ProductFileDefinition> validRecords = records.Where(eactRecord => eactRecord.IsValid()).ToList();

                List<Product> productItems = validRecords.Select(eachValidItem =>
                new Product
                {
                    AmazonUrl = eachValidItem.AmazonLink!,
                    ASIN = eachValidItem.Asin!,
                    Barcode = eachValidItem.Barcode!,
                    IsDeleted = false,
                    Name = eachValidItem.Name!,
                    RequestedPriceDifferenceWithPercentage = eachValidItem.Percentage!,
                    TrendyolUrl = eachValidItem.TrendyolLink!
                }).ToList();

                await DbContext.Products.AddRangeAsync(productItems);
                await DbContext.SaveChangesAsync();
            }
        }
    }

    public async Task SendPriceAlertEmail()
    {
        //List<ScraperVisit> itemsToSend = await DbContext.ScraperVisits.Where(visit => visit.NeedToNotify && !visit.Notified).ToListAsync();
        var items = await (from visit in DbContext.ScraperVisits.AsNoTracking()
                           join product in DbContext.Products.AsNoTracking() on visit.ProductId equals product.Id
                           where visit.NeedToNotify && !visit.Notified
                           select new
                           {
                               visit,
                               product
                           }).ToListAsync();
        
        string rowTemplateFileName = "PriceItemTemplate.txt";
        string rowFilepath = Path.Combine(Environment.CurrentDirectory, @"Assets\EmailTemplates\", rowTemplateFileName);
        string rowFileContent = System.IO.File.ReadAllText(rowFilepath, System.Text.Encoding.UTF8);

        string htlmTemplateFileName = "PriceAlertTemplate.txt";
        string hmtlFilepath = Path.Combine(Environment.CurrentDirectory, @"Assets\EmailTemplates\", htlmTemplateFileName);
        string htmlFileContent = System.IO.File.ReadAllText(hmtlFilepath, System.Text.Encoding.UTF8);

        StringBuilder rowTemplateBuilder = new StringBuilder();
        items.ForEach((info) => {
            string rowTemplate = rowFileContent
            .Replace("##id", info.visit.ProductId.ToString())
            .Replace("##name", info.product.Name)
            .Replace("##amazonPreviousPrice", info.visit.AmazonPreviousPrice.ToString())
            .Replace("##amazonCurrentPrice", info.visit.AmazonCurrentPrice.ToString())
            .Replace("##trendyolPreviousPrice", info.visit.TrendyolPreviousPrice.ToString())
            .Replace("##trendyolCurrentPrice", info.visit.TrendyolCurrentPrice.ToString())
            .Replace("##calculatedDifferenceAsAmount", info.visit.CalculatedPriceDifferenceAsAmount.ToString())
            .Replace("##calculatedDifferenceAsPercentage", info.visit.CalculatedPriceDifferenceAsPercentage.ToString())
            .Replace("##requestedDifferenceAsAmount", info.product.RequestedPriceDifferenceWithAmount.ToString())
            .Replace("##requestedDifferenceAsPercentage", info.product.RequestedPriceDifferenceWithPercentage.ToString());
            
            rowTemplateBuilder.Append(rowTemplate);
        });

        string rowHtml = rowTemplateBuilder.ToString();
        htmlFileContent = htmlFileContent.Replace("##rowTemplate", rowHtml);
        //itemsToSend.ForEach((visit) => visit.Notified = true);
        //await DbContext.SaveChangesAsync();

        var message = new MailMessage(new string[] { "necatikaya86@hotmail.com", "fmuratkaya@hotmail.com" }, "Prices Are Changing - Good Luck", htmlFileContent);
        await MailSender.SendEmail(message);
    }
}