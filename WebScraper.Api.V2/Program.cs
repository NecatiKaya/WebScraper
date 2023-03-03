using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Net;
using WebScraper.Api.V2.Business;
using WebScraper.Api.V2.Business.Email;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Jobs;
using WebScraper.Api.V2.Repositories;

var builder = WebApplication.CreateBuilder(args);
string? connectionString = builder.Configuration.GetConnectionString("default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new ArgumentNullException(nameof(connectionString));
}

string? logDbConnectionString = builder.Configuration.GetConnectionString("logDb");
if (string.IsNullOrWhiteSpace(logDbConnectionString))
{
    throw new ArgumentNullException(nameof(logDbConnectionString));
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<WebScraperDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<WebScraperLogDbContext>(options => options.UseSqlServer(logDbConnectionString));
builder.Services.AddCors((corsOptions) =>
{
    corsOptions.AddPolicy(name: "_allowedHosts",
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:4200", "http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                      });
});

MailConfiguration? emailConfig = builder.Configuration
        .GetSection("EmailConfiguration")
        .Get<MailConfiguration>();
if (emailConfig is not null)
{
    builder.Services.AddSingleton(emailConfig);
}

builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ScraperVisitRepository>();
builder.Services.AddScoped<IMailSender, MailSender>();
builder.Services.AddScoped<AlertingBusiness, AlertingBusiness>();

builder.Services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();

FlurlHttp.ConfigureClient("https://www.amazon.com.tr", (settings) =>
{
    //settings.Configure((globalSettings) =>
    //{
    //    globalSettings.HttpClientFactory = new ProxyHttpClientFactory("_spider_", "91b871-ad0b42-17c6fd-734129-7bb591", "http://premium.residential.proxyrack.net:9000");
    //    globalSettings.HttpClientFactory = new ProxyHttpClientFactory("_spider_", "91b871-ad0b42-17c6fd-734129-7bb591", "http://private.residential.proxyrack.net:10000");
    //});

    settings.BeforeCall(call =>
    {
        call.Request.CookieJar?.Clear();        
        Console.WriteLine($"Calling {call.Request.Url}");

        //call.LogRequestCallAsync();
    });
    settings.AfterCall(call =>
    {
        Console.WriteLine($"Called {call.Request.Url}");

        //call.LogResponseCallAsync();
    });
    settings.OnError(call =>
    {
        Console.WriteLine($"Call to {call.Request.Url} failed: {call.Exception}");
    });
});

FlurlHttp.ConfigureClient("https://www.trendyol.com", (settings) =>
{
    settings.BeforeCall(call =>
    {
        Console.WriteLine($"Calling {call.Request.Url}");

        //call.LogRequestCallAsync();
    });
    settings.AfterCall(call =>
    {
        Console.WriteLine($"Called {call.Request.Url}");

        //call.LogResponseCallAsync();
    });
    settings.OnError(call =>
    {
        Console.WriteLine($"Call to {call.Request.Url} failed: {call.Exception}");
    });
});

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    JobKey crawlJobKey = new JobKey("CrawlJob");
    q.AddJob<CrawlJob>(opts => opts.WithIdentity(crawlJobKey));
    q.AddTrigger(opts => opts
        .ForJob(crawlJobKey)
        .WithIdentity("CrawlJob-Trigger")
        .WithSimpleSchedule(x => x
            .WithInterval(TimeSpan.FromMinutes(15))
            .RepeatForever()));

    //JobKey cookieLoadJobJobKey = new JobKey("LoadCookiesJob");
    //q.AddJob<LoadCookiesJob>(opts => opts.WithIdentity(cookieLoadJobJobKey));
    //q.AddTrigger(opts => opts
    //    .ForJob(cookieLoadJobJobKey)
    //    .WithIdentity("LoadCookiesJob-Trigger")
    //    .WithSimpleSchedule(x => x
    //        .WithInterval(TimeSpan.FromSeconds(10))
    //        .RepeatForever()));

    //JobKey crawlJobWithPuppeteerOnlyAmazon = new JobKey("CrawlJobWithPuppeteerOnlyAmazon");
    //q.AddJob<CrawlJobWithPuppeteerOnlyAmazon>(opts => opts.WithIdentity(crawlJobWithPuppeteerOnlyAmazon));
    //q.AddTrigger(opts => opts
    //    .ForJob(crawlJobWithPuppeteerOnlyAmazon)
    //    .WithIdentity("CrawlJobWithPuppeteerOnlyAmazon-Trigger")
    //    .WithSimpleSchedule(x => x
    //        .WithInterval(TimeSpan.FromMinutes(45))
    //        .RepeatForever()));
});

builder.Services.AddQuartzHostedService(q =>
{
    q.AwaitApplicationStarted = true;
    q.WaitForJobsToComplete = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("_allowedHosts");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

app.Run();