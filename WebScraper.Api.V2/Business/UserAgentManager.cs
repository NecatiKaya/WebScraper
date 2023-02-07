using Flurl.Http;
using WebScraper.Api.V2.Business.Parsing;
using WebScraper.Api.V2.Data.Dto.UserAgentString;

namespace WebScraper.Api.V2.Business;

public static class UserAgentManager
{
    const string USER_AGENTS_STRING_URL = "https://useragentstring.com/";

    /// <summary>
    /// https://useragentstring.com/pages/All/ downloads user agent strings 
    /// </summary>
    public static async Task<List<UserAgentStringDto>> DownloadAllAgents()
    {
        IFlurlResponse response = await (USER_AGENTS_STRING_URL + "pages/All/").GetAsync();
        response.ResponseMessage.EnsureSuccessStatusCode();
        string html = await response.GetStringAsync();

        IUserAgentStringsParser parser = new UserAgentStringsParser(html);
        List<UserAgentStringDto> allUserAgents = await parser.ParseUserAgents();
        return allUserAgents;
    }

    public static async Task<List<UserAgentStringDto>> DownloadSpecificAgents(string urlPart)
    {
        //urlPart = "_uas_AOL_version_8.0.php";
        IFlurlResponse response = await (USER_AGENTS_STRING_URL + urlPart).GetAsync();
        response.ResponseMessage.EnsureSuccessStatusCode();
        string html = await response.GetStringAsync();

        IUserAgentStringsParser parser = new UserAgentStringsParser(html);
        return await parser.ParseUserAgents();
    }
}