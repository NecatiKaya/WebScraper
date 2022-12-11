namespace WebScraper.Api.V2.Data.Dto;

public class ServerPagingRequest
{
    public ServerPagingRequest()
    {
        Params = new List<KeyValuePair<string, object>>();
    }

    public string SortKey { get; set; } = "id";

    public string SortDirection { get; set; } = "asc";

    public int PageSize { get; set; }

    public int PageIndex { get; set; }

    public List<KeyValuePair<string, object>> Params { get; set; }

    public object? GetParamValue(string name)
    {
        KeyValuePair<string, object>? param = Params.Where(param => param.Key == name).FirstOrDefault();
        if (param != null)
        {
            return param.Value.Value;
        }

        return null;
    }
}