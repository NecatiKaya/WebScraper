using Microsoft.EntityFrameworkCore;

namespace WebScraper.Api.V2.Data.Models;

public class WebScraperDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public DbSet<ScraperVisit> ScraperVisits { get; set; }

    public DbSet<UserAgentString> UserAgentStrings { get; set; }

    public DbSet<CookieStore> CookieStores { get; set; }

    public DbSet<ApplicationLog> ApplicationLogs { get; set; }

    public WebScraperDbContext(DbContextOptions options) : base(options) { }

    public WebScraperDbContext()
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            var connectionString = configuration.GetConnectionString("default");
            options.UseSqlServer(connectionString);
        }
    }
}