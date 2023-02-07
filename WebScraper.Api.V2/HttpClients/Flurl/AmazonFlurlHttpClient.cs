﻿using Flurl.Http;
using Flurl.Http.Configuration;
using System.Net;
using System.Text.Json;
using System.Threading;
using WebScraper.Api.V2.Business;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Exceptions;
using WebScraper.Api.V2.Logging;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.HttpClients.Flurl;

public sealed class AmazonFlurlHttpClient : CrawlerHttpClientBase, IDisposable
{
    private IFlurlClient? _flurlClient;

    private readonly WebScraperDbContext _dbContext;

    private UserAgentStringRepository? userAgentRepository;

    private CookieStoreRepository? cookieStoreRepository;

    private readonly IFlurlClientFactory _flurlClientFac;

    private bool isConfigured = false;

    private bool disposedValue;

    public AmazonFlurlHttpClient(IFlurlClientFactory flurlClientFac, ClientConfiguration clientConfiguration, WebScraperDbContext context) : base(clientConfiguration)
    {
        _flurlClientFac = flurlClientFac;
        _dbContext = context;
    }

    public override void Configure()
    {
        _flurlClient = _flurlClientFac.Get("https://www.amazon.com.tr/");
        userAgentRepository = new UserAgentStringRepository(_dbContext);
        cookieStoreRepository = new CookieStoreRepository(_dbContext);
        isConfigured = true;
    }

    public override Task ConfigureAsync()
    {
        Configure();
        return Task.CompletedTask;
    }

    public override HttpClientResponse? Crawl(Product product, string? cookie, string? userAgent)
    {
        throw new NotImplementedException("AmazonFlurlHttpClient.Crawl() is not implemented. Please make use of one of CrawlAsync(...) methods");
    }

    public override async Task<HttpClientResponse?> CrawlAsync(Product product, string? cookie, string? userAgent, CancellationToken cancellationToken = default)
    {
        string jobId = Options?.LoggingJar?.JobId ?? "#JOBID#NOT#SPECIFIED";
        string jobName = Options?.LoggingJar?.JobName?? "#JOBNAME#NOT#SPECIFIED";
        string transactionId = Options?.LoggingJar?.TransactionId ?? "#TRANSACTIONID#NOT#SPECIFIED";

        ScraperVisitRepository scraperVisitRepository = new ScraperVisitRepository(_dbContext);
        ScraperVisit scraperVisit = new ScraperVisit()
        {
            NeedToNotify = false,
            Notified = false,
            ProductId = product.Id,
            VisitDate = DateTime.Now
        };

        if (!isConfigured)
        {
            throw new HttpClientNotConfiguredException(Websites.Amazon);
        }

        if (product is null || string.IsNullOrWhiteSpace(product.AmazonUrl))
        {
            throw new CrawlingUrlNotFoundException(Websites.Amazon, null);
        }

        DateTime start = DateTime.Now;

        try
        {
            IFlurlRequest req = new FlurlRequest(product.AmazonUrl);

            if (!string.IsNullOrEmpty(userAgent))
            {
                req = req.WithHeader("user-agent", userAgent);
            }

            if (!string.IsNullOrEmpty(cookie))
            {
                req = req.WithHeader("cookie", cookie);
            }

            req = req
                    .WithHeader("accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7")
                    .WithHeader("pragma", "no-cache")
                    .WithHeader("sec-ch-ua-platform", "Windows")
                    .WithHeader("upgrade-insecure-requests", "1")
                    .WithHeader("ect", "4g")
                    .WithHeader("cache-control", "no-cache")
                    .WithHeader("referer", "https://www.amazon.com.tr");

            IFlurlResponse response = await req.GetAsync(cancellationToken, HttpCompletionOption.ResponseContentRead);
            response.ResponseMessage.EnsureSuccessStatusCode();
            string html = await response.GetStringAsync();

            await scraperVisitRepository.AddVisitsAsync(new ScraperVisit[] { scraperVisit });

            HttpClientCookie[]? cookies = GetCookies(response);
            HttpStatusCode statusCode = (HttpStatusCode)response.StatusCode;
            IReadOnlyDictionary<string, string> requestHeaders = response.GetHeadersAsDictionary(false);
            IReadOnlyDictionary<string, string> responseHeaders = response.GetHeadersAsDictionary(true);

            ApplicationLog applicationLog = ApplicationLogBusiness.CreateInformationLog($"Product ('{product.Id} - {product.Name}') has been crawled.", jobName, jobId, transactionId, start, DateTime.Now - start, product.AmazonUrl, product.Id, GetRequestId(), html, JsonSerializer.Serialize(requestHeaders), JsonSerializer.Serialize(responseHeaders), scraperVisit.Id, response.StatusCode);

            await AddLogAsync(applicationLog);

            if (html.ToLower().Contains("sadece robot olma"))
            {               
                throw new AmazonBotDetectedException(product.AmazonUrl, product.Id, null, html, null, null);
            }

            HttpClientResponse clientResponse = new HttpClientResponse(statusCode, statusCode.ToString(), html, GetRequestId(), requestHeaders, responseHeaders, cookies, scraperVisit.Id);
            return clientResponse;
        }
        catch (AmazonBotDetectedException botDetectedEx)
        {
            scraperVisit.AmazonPriceNotFoundReason = PriceNotFoundReasons.BotDetected;
            await scraperVisitRepository.UpdateVisit();

            ApplicationLog botDetectedLog = await ApplicationLogBusiness.CreateErrorLogFromExceptionAsync("AmazonBotDetectedException is occured in AmazonFlurlHttpClient.CrawlAsync method",
                Options?.LoggingJar?.JobName ?? "AmazonFlurlHttpClient.CrawlAsync",
                Options?.LoggingJar?.JobId ?? GetRequestId(),
                null,
                botDetectedEx,
                start,
                DateTime.Now,
                product.AmazonUrl,
                GetRequestId(),
                product.Id,
                visitId: scraperVisit.Id);

            if (Options is not null && Options.LoggingJar is not null)
            {
                await Options.LoggingJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(botDetectedLog), force: true);
            }

            HttpStatusCode? statusCode = botDetectedLog.StatusCode.HasValue ? (HttpStatusCode)botDetectedLog.StatusCode : null;
            string? statusText = statusCode?.ToString() ?? null;
            return new HttpClientResponse(statusCode, statusText, null, GetRequestId(), null, null, null, scraperVisit.Id);
        }
        catch (FlurlHttpTimeoutException timeoutEx)
        {
            scraperVisit.AmazonPriceNotFoundReason = PriceNotFoundReasons.ExceptionOccured;
            await scraperVisitRepository.AddVisitsAsync(new ScraperVisit[] { scraperVisit });

            ApplicationLog timeoutErrorLog = await ApplicationLogBusiness.CreateErrorLogFromExceptionAsync("FlurlHttpTimeoutException is occured in AmazonFlurlHttpClient.CrawlAsync method",
                Options?.LoggingJar?.JobName ?? "AmazonFlurlHttpClient.CrawlAsync",
                Options?.LoggingJar?.JobId ?? GetRequestId(),
                null,
                timeoutEx,
                start,
                DateTime.Now,
                product.AmazonUrl,
                GetRequestId(),
                product.Id,
                visitId: scraperVisit.Id,
                statusCode: timeoutEx.StatusCode);

            if (Options is not null && Options.LoggingJar is not null)
            {
                await Options.LoggingJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(timeoutErrorLog), force: true);
            }

            HttpStatusCode? statusCode = timeoutErrorLog.StatusCode.HasValue ? (HttpStatusCode)timeoutErrorLog.StatusCode : null;
            string? statusText = statusCode?.ToString() ?? null;
            /*Most propably response headers will be null due to this is a timeout ex*/
            return new HttpClientResponse(statusCode, statusText, null, GetRequestId(), timeoutEx.Call?.Response?.GetHeadersAsDictionary(false), timeoutEx.Call?.Response?.GetHeadersAsDictionary(true), null, scraperVisit.Id);
        }
        catch (FlurlHttpException httpEx)
        {
            scraperVisit.AmazonPriceNotFoundReason = PriceNotFoundReasons.ExceptionOccured;
            await scraperVisitRepository.AddVisitsAsync(new ScraperVisit[] { scraperVisit });

            ApplicationLog httpErrorLog = await ApplicationLogBusiness.CreateErrorLogFromExceptionAsync("FlurlHttpException is occured in in AmazonFlurlHttpClient.CrawlAsync method",
                Options?.LoggingJar?.JobName ?? "AmazonFlurlHttpClient.CrawlAsync",
                Options?.LoggingJar?.JobId ?? GetRequestId(),
                null,
                httpEx,
                start,
                DateTime.Now,
                product.AmazonUrl,
                GetRequestId(),
                product.Id,
                visitId: scraperVisit.Id,
                statusCode: httpEx.StatusCode);

            if (Options is not null && Options.LoggingJar is not null)
            {
                await Options.LoggingJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(httpErrorLog));
            }

            HttpStatusCode? statusCode = httpErrorLog.StatusCode.HasValue ? (HttpStatusCode)httpErrorLog.StatusCode : null;
            string? statusText = statusCode?.ToString() ?? null;

            return new HttpClientResponse(statusCode, statusText, httpErrorLog.ResponseHtml, GetRequestId(), httpEx.Call?.Response?.GetHeadersAsDictionary(false), httpEx.Call?.Response?.GetHeadersAsDictionary(true), null, scraperVisit.Id);
        }
        catch (Exception ex)
        {
            scraperVisit.AmazonPriceNotFoundReason = PriceNotFoundReasons.ExceptionOccured;
            await scraperVisitRepository.AddVisitsAsync(new ScraperVisit[] { scraperVisit });

            ApplicationLog genericErrorApplicationLog = await ApplicationLogBusiness.CreateErrorLogFromExceptionAsync("Unknow exception is occured in in AmazonFlurlHttpClient.CrawlAsync method",
                Options.LoggingJar?.JobName ?? "AmazonFlurlHttpClient.CrawlAsync",
                Options.LoggingJar?.JobId ?? GetRequestId(),
                null,
                ex,
                start,
                DateTime.Now,
                product.AmazonUrl,
                GetRequestId(),
                product.Id,
                visitId: scraperVisit.Id);

            if (Options is not null && Options.LoggingJar is not null)
            {
                await Options.LoggingJar.AddLogAndSaveIfNeedAsync(new ApplicationLogModel(genericErrorApplicationLog));
            }

            HttpStatusCode? statusCode = genericErrorApplicationLog.StatusCode.HasValue ? (HttpStatusCode)genericErrorApplicationLog.StatusCode : null;
            string? statusText = statusCode?.ToString() ?? null;

            return new HttpClientResponse(statusCode, statusText, genericErrorApplicationLog.ResponseHtml, GetRequestId(), null, null, null, scraperVisit.Id);
        }
    }

    private HttpClientCookie[]? GetCookies(IFlurlResponse response)
    {
        HttpClientCookie[]? cookies = response.Cookies
                       .Where(cookie => cookie.Domain?.Contains("amazon.com.tr") == true)
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

    private async Task AddLogAsync(ApplicationLog log)
    {
        await new ApplicationLogBusiness(new ApplicationLogRepository(_dbContext)).AddErrorAsync(log);
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