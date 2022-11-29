namespace WebScraper.Api.Dto.UserAgent
{
    public class UserAgentStringsTree
    {
        public UserAgentStringsTree() 
        {

        }

        public List<UserAgentStringDto> Children { get; set; } = new List<UserAgentStringDto>();
    }
}
