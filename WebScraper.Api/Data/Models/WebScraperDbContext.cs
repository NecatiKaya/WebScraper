﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace WebScraper.Api.Data.Models
{
    public class WebScraperDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public DbSet<ScraperVisit> ScraperVisits { get; set; }

        public DbSet<HttpError> HttpErrorLogs { get; set; }

        public DbSet<AppLog> AppLogs { get; set; }

        public DbSet<UserAgentString> UserAgentStrings { get; set; }

        public DbSet<CookieStore> CookieStores { get; set; }

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
}