using System.Text.Json;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Exceptions;
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

    public static ApplicationLog CreateErrorLogFromException(string description, string jobName, string jobId, Exception ex, DateTime? operationStartDate = null, DateTime? errorDate = null, string? url = null, string? requestId = null)
    {
        DateTime nowDate = DateTime.Now;
        TimeSpan? duration = null;

        ApplicationLog errorLog = new ApplicationLog()
        {
            JobName = jobName,
            JobId = jobId.ToString(),
            Date = operationStartDate ?? errorDate ?? nowDate,
            Description = description,
            Level = LogLevel.Error,
            ErrorMessage = ex.Message,
            ExceptionType = ex.GetType().Name,
            StackTrace = ex.StackTrace,
        };

        if (operationStartDate is not null)
        {
            if (errorDate is not null)
            {
                duration = errorDate - operationStartDate;
            }
            else
            {
                duration = nowDate - operationStartDate;
            }

            errorLog.Duration = duration;
        }

        if (ex is WebScrapingException)
        {
            WebScrapingException _ex = ex as WebScrapingException;
            errorLog.Url = _ex.Url;
            errorLog.TransactionId = requestId;
            errorLog.ProductId = _ex.ProductId;
            errorLog.RequestHeadersAsString = ConvertToJson(_ex.HttpRequestHeaders);
            errorLog.ResponseHeadersAsString = ConvertToJson(_ex.HttpResponseHeaders);
            errorLog.ResponseHtml = _ex.HttpResponse;
            errorLog.StatusCode = _ex.StatusCode;
            errorLog.ScraperVisitId = _ex.ScraperVisitId;
        }

        return errorLog;
    }

    public static ApplicationLog CreateInformationLog(string description, string jobName, string jobId, DateTime date, TimeSpan? duration = null)
    {
        ApplicationLog appLog = new ApplicationLog();
        appLog.JobName = jobName;
        appLog.JobId = jobId.ToString();
        appLog.Date = date;
        appLog.Description = description;
        appLog.Level = LogLevel.Information;
        appLog.Duration = duration;

        return appLog;
    }

    private static string? ConvertToJson(object data)
    {
        if (data is null)
        {
            return null;
        }

        JsonSerializerOptions options = new JsonSerializerOptions();
        options.AllowTrailingCommas = false;
        options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.WriteAsString;
        options.WriteIndented = false;
        string? headersAsString = JsonSerializer.Serialize(data, options);
        return headersAsString;
    }
}