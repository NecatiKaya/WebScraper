using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Logging;

public class ApplicationLogModelJar
{
    private List<ApplicationLogModel> _applicationLogModels = new List<ApplicationLogModel>();

    public string? JobName { get; set; }

    public string? JobId { get; set; }

    public string? TransactionId { get; set; }

    public delegate void ApplicationLogModelHandler (object sender, ApplicationLogModelEventArgs e);

    public event ApplicationLogModelHandler? LogAdded;

    protected WebScraperDbContext _dbContext { get; set; }

    protected ILogger ConsoleLogger { get; set; }

    public ApplicationLogModelJar(WebScraperDbContext context, ILogger consoleLogger)
    {
        _dbContext = context;
        ConsoleLogger = consoleLogger;
    }

    public void AddLog(ApplicationLogModel model, bool saveToConsole = true)
    {
        _applicationLogModels.Add(model);
        OnAddLogEvent(new ApplicationLogModelEventArgs(model));
        if (saveToConsole)
        {
            ConsoleLogger.LogInformation(model.AppLog.Description);
        }
    }

    public async Task AddLogAndSaveIfNeedAsync(ApplicationLogModel model, bool saveToConsole = true, bool force = false)
    {
        AddLog(model, saveToConsole);
        await SaveAppLogsIfNeededAsync(force);
    }

    public List<ApplicationLogModel> GetAllLogs()
    {
        return new List<ApplicationLogModel>(_applicationLogModels);
    }

    public void Clear()
    {
        _applicationLogModels.Clear();
    }

    protected virtual void OnAddLogEvent(ApplicationLogModelEventArgs args)
    {
        ApplicationLogModelHandler? logAdded = LogAdded;
        logAdded?.Invoke(this, args);
    }

    public async Task SaveAppLogsIfNeededAsync(bool force = false)
    {
        if (_applicationLogModels?.Count >= 50 || (force && _applicationLogModels!.Any()))
        {
            await _dbContext.ApplicationLogs.AddRangeAsync(_applicationLogModels!.Select(x => x.AppLog));
            await _dbContext.SaveChangesAsync();

            Clear();
        }
    }
}
