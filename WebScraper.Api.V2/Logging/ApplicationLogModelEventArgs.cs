namespace WebScraper.Api.V2.Logging;

public class ApplicationLogModelEventArgs : EventArgs
{
	public ApplicationLogModelEventArgs(ApplicationLogModel applicationLogModel)
    {
        ApplicationLogModel = applicationLogModel;
    }

    public ApplicationLogModel ApplicationLogModel { get; set; }
}
