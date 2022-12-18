namespace WebScraper.Api.V2.Logging;

public class ApplicationLogModelJar
{
    private List<ApplicationLogModel> _applicationLogModels = new List<ApplicationLogModel>();

    public string? JobName { get; set; }

    public string? JobId { get; set; }

    public delegate void ApplicationLogModelHandler (object sender, ApplicationLogModelEventArgs e);

    public event ApplicationLogModelHandler? LogAdded;

    public ApplicationLogModelJar()
    {
        
    }

    public void AddLog(ApplicationLogModel model)
    {
        _applicationLogModels.Add(model);
        OnAddLogEvent(new ApplicationLogModelEventArgs(model));
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
}
