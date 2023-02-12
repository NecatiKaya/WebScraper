using Flurl.Http;
using Flurl.Http.Configuration;
using System.Net;
using System.Text.Json;
using WebScraper.Api.V2.Business;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Exceptions;
using WebScraper.Api.V2.Logging;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.HttpClients.Flurl;

public class TrendyolFlurlHttpClient : CrawlerHttpClientBase, IDisposable
{
    private IFlurlClient? _flurlClient;

    private readonly WebScraperDbContext _dbContext;

    private readonly WebScraperLogDbContext _logDbContext;

    private UserAgentStringRepository? userAgentRepository;

    private CookieStoreRepository? cookieStoreRepository;

    private readonly IFlurlClientFactory _flurlClientFac;

    private bool isConfigured = false;

    private bool disposedValue;

    public TrendyolFlurlHttpClient(IFlurlClientFactory flurlClientFac, ClientConfiguration clientConfiguration, WebScraperDbContext context, WebScraperLogDbContext logDbContext) : base(clientConfiguration)
    {
        _flurlClientFac = flurlClientFac;
        _dbContext = context;
        _logDbContext = logDbContext;
    }

    public override void Configure()
    {
        _flurlClient = _flurlClientFac.Get("https://www.trendyol.com/");
        userAgentRepository = new UserAgentStringRepository(_logDbContext);
        cookieStoreRepository = new CookieStoreRepository(_logDbContext);
        isConfigured = true;
    }

    public override Task ConfigureAsync()
    {
        Configure();
        return Task.CompletedTask;
    }

    public override HttpClientResponse? Crawl(Product product, string? cookie, string? userAgent)
    {
        throw new NotImplementedException("TrendyolFlurlHttpClient.Crawl() is not implemented. Please make use of one of CrawlAsync(...) methods");
    }

    public override async Task<HttpClientResponse?> CrawlAsync(Product product, string? cookie, string? userAgent, CancellationToken cancellationToken = default)
    {
        string jobId = Options?.LoggingJar?.JobId ?? "#JOBID#NOT#SPECIFIED";
        string jobName = Options?.LoggingJar?.JobName ?? "#JOBNAME#NOT#SPECIFIED";
        string transactionId = Options?.LoggingJar?.TransactionId ?? "#TRANSACTIONID#NOT#SPECIFIED";

        if (!isConfigured)
        {
            throw new HttpClientNotConfiguredException(Websites.Trendyol);
        }

        if (product is null || string.IsNullOrWhiteSpace(product.TrendyolUrl))
        {
            throw new CrawlingUrlNotFoundException(Websites.Trendyol, null);
        }

        DateTime start = DateTime.Now;
        
        try
        {
            IFlurlRequest req = new FlurlRequest(product.TrendyolUrl);

            IFlurlResponse response = await req.GetAsync(cancellationToken, HttpCompletionOption.ResponseContentRead);
            //response.ResponseMessage.EnsureSuccessStatusCode();
            string html = await response.GetStringAsync();

            HttpClientCookie[]? cookies = GetCookies(response);
            HttpStatusCode statusCode = (HttpStatusCode)response.StatusCode;
            IReadOnlyDictionary<string, string> requestHeaders = response.GetHeadersAsDictionary(false);
            IReadOnlyDictionary<string, string> responseHeaders = response.GetHeadersAsDictionary(true);

            ///TODO: transactionid parametresi güncellenmeli
            ApplicationLog applicationLog = ApplicationLogBusiness.CreateInformationLog($"Product ('{product.Id} - {product.Name}') has been crawled.", jobName, jobId, transactionId, start, DateTime.Now - start, product.TrendyolUrl, product.Id, GetRequestId(), html, JsonSerializer.Serialize(requestHeaders), JsonSerializer.Serialize(responseHeaders), null, response.StatusCode);

            if (statusCode == HttpStatusCode.TooManyRequests)
            {
                throw new TooManyRequestException(Websites.Trendyol, product.TrendyolUrl, product.Id, null, html, requestHeaders.ToList(), responseHeaders.ToList());
            }

            HttpClientResponse clientResponse = new HttpClientResponse(statusCode, statusCode.ToString(), html, GetRequestId(), requestHeaders, responseHeaders, cookies);
            return clientResponse;
        }
        catch (TooManyRequestException tooManyRequestEx)
        {
            ApplicationLog tooManyRequestsLog = await ApplicationLogBusiness.CreateErrorLogFromExceptionAsync("TooManyRequestException is occured in TrendyolFlurlHttpClient.CrawlAsync method",
                Options?.LoggingJar?.JobName ?? "TrendyolFlurlHttpClient.CrawlAsync",
                Options?.LoggingJar?.JobId ?? GetRequestId(),
                transactionId,
                tooManyRequestEx,
                start,
                DateTime.Now,
                product.AmazonUrl,
                GetRequestId(),
                product.Id,
                responseHtml : tooManyRequestEx.HttpResponse,
                requestHeaders: JsonSerializer.Serialize(tooManyRequestEx.HttpRequestHeaders),
                responseHeaders: JsonSerializer.Serialize(tooManyRequestEx.HttpResponseHeaders),
                statusCode : 429);

            if (Options is not null && Options.LoggingJar is not null)
            {
                await Options.LoggingJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(tooManyRequestsLog));
            }

            HttpStatusCode? statusCode = tooManyRequestsLog.StatusCode.HasValue ? (HttpStatusCode)tooManyRequestsLog.StatusCode : null;
            string? statusText = statusCode?.ToString() ?? null;
            return new HttpClientResponse(statusCode, statusText, null, GetRequestId(), null, null, null, true);
        }
        catch (FlurlHttpTimeoutException timeoutEx)
        {
            ApplicationLog timeoutErrorLog = await ApplicationLogBusiness.CreateErrorLogFromExceptionAsync("FlurlHttpTimeoutException is occured in TrendyolFlurlHttpClient.CrawlAsync method",
                Options?.LoggingJar?.JobName ?? "TrendyolFlurlHttpClient.CrawlAsync",
                Options?.LoggingJar?.JobId ?? GetRequestId(),
                transactionId,
                timeoutEx,
                start,
                DateTime.Now,
                product.TrendyolUrl,
                GetRequestId(),
                product.Id,
                statusCode: timeoutEx.StatusCode);

            if (Options is not null && Options.LoggingJar is not null)
            {
                await Options.LoggingJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(timeoutErrorLog));
            }

            HttpStatusCode? statusCode = timeoutErrorLog.StatusCode.HasValue ? (HttpStatusCode)timeoutErrorLog.StatusCode : null;
            string? statusText = statusCode?.ToString() ?? null;
            /*Most propably response headers will be null due to this is a timeout ex*/
            return new HttpClientResponse(statusCode, statusText, null, GetRequestId(), timeoutEx.Call?.Response?.GetHeadersAsDictionary(false), timeoutEx.Call?.Response?.GetHeadersAsDictionary(true), null);
        }
        catch (FlurlHttpException httpEx)
        {
            ApplicationLog httpErrorLog = await ApplicationLogBusiness.CreateErrorLogFromExceptionAsync("FlurlHttpException is occured in in TrendyolFlurlHttpClient.CrawlAsync method",
                Options?.LoggingJar?.JobName ?? "TrendyolFlurlHttpClient.CrawlAsync",
                Options?.LoggingJar?.JobId ?? GetRequestId(),
                transactionId,
                httpEx,
                start,
                DateTime.Now,
                product.TrendyolUrl,
                GetRequestId(),
                product.Id,
                statusCode: httpEx.StatusCode);

            if (Options is not null && Options.LoggingJar is not null)
            {
                await Options.LoggingJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(httpErrorLog));
            }

            HttpStatusCode? statusCode = httpErrorLog.StatusCode.HasValue ? (HttpStatusCode)httpErrorLog.StatusCode : null;
            string? statusText = statusCode?.ToString() ?? null;

            return new HttpClientResponse(statusCode, statusText, httpErrorLog.ResponseHtml, GetRequestId(), httpEx.Call?.Response?.GetHeadersAsDictionary(false), httpEx.Call?.Response?.GetHeadersAsDictionary(true), null);
        }
        catch (Exception ex)
        {
            ApplicationLog genericErrorApplicationLog = await ApplicationLogBusiness.CreateErrorLogFromExceptionAsync("Unknow exception is occured in in TrendyolFlurlHttpClient.CrawlAsync method",
                Options?.LoggingJar?.JobName ?? "TrendyolFlurlHttpClient.CrawlAsync",
                Options?.LoggingJar?.JobId ?? GetRequestId(),
                transactionId,
                ex,
                start,
                DateTime.Now,
                product.AmazonUrl,
                GetRequestId(),
                product.Id);

            if (Options is not null && Options.LoggingJar is not null)
            {
                await Options.LoggingJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(genericErrorApplicationLog));
            }

            HttpStatusCode? statusCode = genericErrorApplicationLog.StatusCode.HasValue ? (HttpStatusCode)genericErrorApplicationLog.StatusCode : null;
            string? statusText = statusCode?.ToString() ?? null;

            return new HttpClientResponse(statusCode, statusText, genericErrorApplicationLog.ResponseHtml, GetRequestId(), null, null, null);
        }
    }

    private HttpClientCookie[]? GetCookies(IFlurlResponse response)
    {
        HttpClientCookie[]? cookies = response.Cookies
                       .Where(cookie => cookie.Domain?.Contains("trendyol.com") == true)
                       .Select(cookie => new HttpClientCookie()
                       {
                           Value = cookie.Value,
                           Domain = cookie.Domain,
                           //Expires = cookie.Expires.Value
                           HttpOnly = cookie.HttpOnly,
                           Name = cookie.Name,
                           Path = cookie.Path,
                           Secure = cookie.Secure,
                           Session = !cookie.HttpOnly,
                           Size = -1,
                           Url = cookie.OriginUrl,
                       }).ToArray();

        return cookies;
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _flurlClient?.Dispose();
            }
            _flurlClient = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
