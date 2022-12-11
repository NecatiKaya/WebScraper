namespace WebScraper.Api.V2.Data.Models;

public class CookieStore
{
    public CookieStore(string cookieValue)
    {
        CookieValue = cookieValue;
    }

    public int Id { get; set; }

    public string CookieValue { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreateDate { get; set; } = DateTime.Now;

    public Websites WebSite { get; set; }
}