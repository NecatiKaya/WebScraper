using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Net;
using WebScraper.Api.Business;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Extensions;
using WebScraper.Api.HttpClients;
using WebScraper.Api.Jobs;
using WebScraper.Api.Utilities;

var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration.GetConnectionString("default");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<WebScraperDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddCors((corsOptions) =>
{
    corsOptions.AddPolicy(name: "_allowedHosts",
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:4200", "http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                      });
});

MailConfiguration emailConfig = builder.Configuration
        .GetSection("EmailConfiguration")
        .Get<MailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IMailSender, MailSender>();
builder.Services.AddScoped<RepositoryBusiness, RepositoryBusiness>();


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
        //call.Request.Headers.Clear();
        //call.Request.Headers.Add("cookie", "session-id=262-4776681-6994001; ubid-acbtr=259-0639309-9668916; lc-acbtr=tr_TR; check=true; x-acbtr=\"9Ky77cpaJZedv01QCuIV@i64OwblLJQ5RLiQzX9KBP5P3UGRqq2cA@xauVglzNwB\"; at-acbtr=Atza|IwEBICY2p6hpyXoCAvm74-aBXBg8GX_ySUjv0kzKrminXCSeBND3KW3-BaQXoyBvFr1LTFzCey65VXndtTpPNqvktEAsc7I5--yLidcxDCH-pJLUfEpb2zlQUBOWwVRGAkTABau07_M2UutmwYoUT-gJnqsduQl6NPiH0o5JSVh1L6nEBznsSKbiu15RsdRwdRnNFGkKwnv_F4_FpF0r-BKa7FQB; sess-at-acbtr=\"EdxvDxQWb+0KL8dhs5ejXcGCSwzvhtNKU2ZEjUxW+RQ=\"; sst-acbtr=Sst1|PQEhEr5OpbbfwFA_r_8022EoCSNtj5OJmScS9Yzbs8zmopzG6Vu0rRm0CDg3w3aDLysTtbm5g4JoenEjR84nFoIjHcWHCn9N_U0RBN15b7BqiqWLyt7OGfDDTYgwlPh9z6bENNkNVb-u7ugZH0E9OJJonDff_lwV1ZkEXgzhAhMVo27uuoMjZzMsEHA3mgbSorRY8jxmiFQRSMNWGhzpH-71BRLDqfT2o77duR8-b2PT7Dyua05JsqdpxwH-aBJDUbuiVqEw_5kiKzHObt8u1GBlH3yyc6Bo_Ju0dwaO79UjnEE; i18n-prefs=TRY; x-amz-captcha-1=1667334871080606; x-amz-captcha-2=DPY4Q5AqRWMSkpDEKJE6cQ==; session-token=/G/v7oO/ty6l4B/hOSWClo+yn4Dfeh5lWuqrDeUgwitjlAWjfZsSQHykZ4bTCFYvZR/wlBPw99En77V+xLorODg1rBB9MiALaPN6NQqdtOd2kYRgB/OcrW3AJa23+JIz63Y268VkESSl1dZadkKm34Nj6RkZQ3jSr2C9T3uEEx3Icm1My/qiAZY1r3ZF0WAi0KWh4LvL3wBbaGN4D2MhAI+YfXyNC89u/4At/xSzGKt5MYVYmsmCdMwaco8acJmi; csm-hit=tb:s-HEETCW865862W41QBT0W|1667327654531&t:1667327657369&adb:adblk_no; session-id-time=2082758401l");
        //call.Request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24\"");
        //call.Request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
        //call.Request.Headers.Add("accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
        //call.Request.Headers.Add("pragma", "no-cache");
        //call.Request.Headers.Add("sec-ch-ua-platform", "Windows");
        //call.Request.Headers.Add("upgrade-insecure-requests", "1");
        //call.Request.Headers.Add("ect", "4g");
        //call.Request.Headers.Add("cache-control", "no-cache");
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

        //await LogHelper.SaveErrorLog(call.Exception, null, call.Request.Url, $"Call to {call.Request.Url} failed: {call.Exception}",);
        call.LogErrorCallAsync();
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

        call.LogErrorCallAsync();
    });
});

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    //JobKey crawlJobKey = new JobKey("CrawlJob");
    //q.AddJob<CrawlJob>(opts => opts.WithIdentity(crawlJobKey));
    //q.AddTrigger(opts => opts
    //    .ForJob(crawlJobKey)
    //    .WithIdentity("CrawlJob-Trigger")
    //    .WithSimpleSchedule(x => x
    //        .WithInterval(TimeSpan.FromMinutes(5))
    //        .RepeatForever()));

    JobKey cookieLoadJobJobKey = new JobKey("LoadCookiesJob");
    q.AddJob<LoadCookiesJob>(opts => opts.WithIdentity(cookieLoadJobJobKey));
    q.AddTrigger(opts => opts
        .ForJob(cookieLoadJobJobKey)
        .WithIdentity("LoadCookiesJob-Trigger")
        .WithSimpleSchedule(x => x
            .WithInterval(TimeSpan.FromSeconds(20))
            .RepeatForever()));
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