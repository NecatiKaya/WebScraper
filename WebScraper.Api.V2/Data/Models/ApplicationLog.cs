using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraper.Api.V2.Data.Models;

public class ApplicationLog
{
    public int Id { get; set; }

    public string? TransactionId { get; set; }

    public string? Description { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorCode { get; set; }

    public string? ExceptionType { get; set; }

    public string? StackTrace { get; set; }

    public string? JobName { get; set; }

    public string? JobId { get; set; }

    public LogLevel Level { get; set; } = LogLevel.Information;

    public string? Url { get; set; }

    public int? ProductId { get; set; }

    public string? ResponseHtml { get; set; }

    [NotMapped]
    public TimeSpan? Duration { get; set; }

    [Column("Duration")]
    public int? DurationInSeconds
    {
        get
        {
            if (Duration is not null)
            {
                return Convert.ToInt32(Duration.Value.TotalSeconds);
            }

            return null;
        }
    }

    public int? StatusCode { get; set; }

    public bool IsTimeoutEx { get; set; } = false;

    public string? ResponseHeadersAsString { get; set; }

    public string? RequestHeadersAsString { get; set; }

    public int? ScraperVisitId { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;
}