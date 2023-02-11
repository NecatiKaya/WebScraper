using Microsoft.EntityFrameworkCore;

namespace WebScraper.Api.V2.Data.Models;

public class WebScraperLogDbContext : DbContext
{
    public DbSet<UserAgentString> UserAgentStrings { get; set; }

    public DbSet<CookieStore> CookieStores { get; set; }

    public DbSet<ApplicationLog> ApplicationLogs { get; set; }

    public WebScraperLogDbContext(DbContextOptions<WebScraperLogDbContext> options) : base(options) { }

    public WebScraperLogDbContext()
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

            var connectionString = configuration.GetConnectionString("logDb");
            options.UseSqlServer(connectionString);
        }
    }
}