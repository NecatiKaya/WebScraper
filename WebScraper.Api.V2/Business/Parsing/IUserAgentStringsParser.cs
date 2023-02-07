using WebScraper.Api.V2.Data.Dto.UserAgentString;

namespace WebScraper.Api.V2.Business.Parsing;

public interface IUserAgentStringsParser
{
    Task<List<UserAgentStringDto>> ParseUserAgents();
}