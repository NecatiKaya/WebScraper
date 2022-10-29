using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraper.Api.Data.Models;

public class HttpError
{
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public string? ErrorUrl { get; set; }

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

    public string? HeadersAsString { get; set; }

    public DateTime ErrorDate { get; set; } = DateTime.Now;
}