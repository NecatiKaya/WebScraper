namespace WebScraper.Api.Exceptions;

public class AmazonBotException : Exception
{
	public string Url { get; set; }

	public AmazonBotException(): base()
	{

	}

	public AmazonBotException(string url): base(url)
	{
		Url = url;
	}
}