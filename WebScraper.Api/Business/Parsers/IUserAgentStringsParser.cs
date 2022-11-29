using WebScraper.Api.Dto.UserAgent;

namespace WebScraper.Api.Business.Parsers;

public interface IUserAgentStringsParser
{
    Task<List<UserAgentStringDto>> ParseUserAgents();
}