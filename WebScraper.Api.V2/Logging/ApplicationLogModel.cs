using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Logging;
public class ApplicationLogModel
{
    public ApplicationLog AppLog { get; set; }

    public ApplicationLogModel(ApplicationLog appLog)
    {
        AppLog = appLog;
    }
}
