using HtmlAgilityPack;
using WebScraper.Api.V2.Data.Dto.UserAgentString;

namespace WebScraper.Api.V2.Business.Parsing;

public class UserAgentStringsParser : IUserAgentStringsParser
{
    public string Html { get; private set; }

    public UserAgentStringsParser(string html)
    {
        if (html is null)
        {
            throw new ArgumentNullException(nameof(html));
        }
        Html = html;
    }

    public async Task<List<UserAgentStringDto>> ParseUserAgents()
    {
        HtmlDocument htmlDoc = new();
        htmlDoc.OptionCheckSyntax = false;
        htmlDoc.LoadHtml(Html);

        HtmlNode? listContainer = htmlDoc.GetElementbyId("liste");
        List<UserAgentStringDto> userAgentStrings = await GetUserAgentStrings(listContainer);

        return userAgentStrings;
    }

    private async Task<List<UserAgentStringDto>> GetUserAgentStrings(HtmlNode? listContainer)
    {
        if (listContainer is null)
        {
            return new List<UserAgentStringDto>();
        }

        List<UserAgentStringDto> userAgentStrings = new List<UserAgentStringDto>();
        string product = null;
        string productWithVersion = null;
        foreach (HtmlNode eachNode in listContainer.ChildNodes)
        {
            if (eachNode.Name == "h3" && eachNode.InnerText != "BROWSERS")
            {
                product = eachNode.InnerText.Trim();
            }
            else if (eachNode.Name == "h4")
            {
                productWithVersion = eachNode.InnerText.Trim();
            }
            else if (eachNode.Name == "ul")
            {
                foreach (HtmlNode agentStringsNode in eachNode.ChildNodes)
                {
                    if (agentStringsNode.Name == "li")
                    {
                        userAgentStrings.Add(new UserAgentStringDto(agentStringsNode.InnerText)
                        {
                            Product = product,
                            Version = productWithVersion
                        });
                    }
                    if (agentStringsNode.Name == "a")
                    {
                        string link = agentStringsNode.GetAttributeValue("href", null);
                        userAgentStrings.AddRange(await UserAgentManager.DownloadSpecificAgents(link));
                    }
                }
            }
        }

        return userAgentStrings;
    }
}