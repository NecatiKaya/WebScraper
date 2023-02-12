namespace WebScraper.Api.V2.Data.Models;

public class CookieStore
{
    public CookieStore(string cookieName, string cookieValue)
    {
        CookieValue = cookieValue;
        CookieName = cookieName;
    }

    public int Id { get; set; }

    public string CookieValue { get; set; }

    public string CookieName { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreateDate { get; set; } = DateTime.Now;

    public Websites WebSite { get; set; }
}