using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Net;
using WebScraper.Api.Business.Email;
using WebScraper.Api.Data.Models;

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

//builder.Services.AddQuartz(q =>
//{
//    q.UseMicrosoftDependencyInjectionJobFactory();

//    JobKey crawlJobKey = new JobKey("CrawlJob");
//    q.AddJob<CrawlJob>(opts => opts.WithIdentity(crawlJobKey));
//    q.AddTrigger(opts => opts
//        .ForJob(crawlJobKey)
//        .WithIdentity("CrawlJob-Trigger")
//        .WithSimpleSchedule(x => x
//            .WithInterval(TimeSpan.FromMinutes(1))
//            .RepeatForever()));

//    JobKey priceAlertJobKey = new JobKey("PriceAlertJob");
//    q.AddJob<PriceAlertJob>(opts => opts.WithIdentity(priceAlertJobKey));
//    q.AddTrigger(opts => opts
//        .ForJob(priceAlertJobKey)
//        .WithIdentity("PriceAlertJob-Trigger")
//        .WithSimpleSchedule(x => x
//            .WithInterval(TimeSpan.FromMinutes(2))
//            .RepeatForever()));
//});

//builder.Services.AddQuartzHostedService(q =>
//{
//    q.AwaitApplicationStarted = true;
//    q.WaitForJobsToComplete = true;
//});

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