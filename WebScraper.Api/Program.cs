using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Net;
using WebScraper.Api.Business;
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

//FlurlHttp.Configure((settings) =>
//{
//    settings.BeforeCall = call =>
//    {
//        call.Request.Headers.Clear();
//        if (call.Request.Url.ToString().Contains("amazon.com.tr"))
//        {
//            call.Request.Headers.Add("cookie", "session-id=262-4776681-6994001; ubid-acbtr=259-0639309-9668916; lc-acbtr=tr_TR; check=true; x-acbtr=\"9Ky77cpaJZedv01QCuIV@i64OwblLJQ5RLiQzX9KBP5P3UGRqq2cA@xauVglzNwB\"; at-acbtr=Atza|IwEBICY2p6hpyXoCAvm74-aBXBg8GX_ySUjv0kzKrminXCSeBND3KW3-BaQXoyBvFr1LTFzCey65VXndtTpPNqvktEAsc7I5--yLidcxDCH-pJLUfEpb2zlQUBOWwVRGAkTABau07_M2UutmwYoUT-gJnqsduQl6NPiH0o5JSVh1L6nEBznsSKbiu15RsdRwdRnNFGkKwnv_F4_FpF0r-BKa7FQB; sess-at-acbtr=\"EdxvDxQWb+0KL8dhs5ejXcGCSwzvhtNKU2ZEjUxW+RQ=\"; sst-acbtr=Sst1|PQEhEr5OpbbfwFA_r_8022EoCSNtj5OJmScS9Yzbs8zmopzG6Vu0rRm0CDg3w3aDLysTtbm5g4JoenEjR84nFoIjHcWHCn9N_U0RBN15b7BqiqWLyt7OGfDDTYgwlPh9z6bENNkNVb-u7ugZH0E9OJJonDff_lwV1ZkEXgzhAhMVo27uuoMjZzMsEHA3mgbSorRY8jxmiFQRSMNWGhzpH-71BRLDqfT2o77duR8-b2PT7Dyua05JsqdpxwH-aBJDUbuiVqEw_5kiKzHObt8u1GBlH3yyc6Bo_Ju0dwaO79UjnEE; i18n-prefs=TRY; session-token=uBAN9RkDx83qQd/DH48K/A74zgw8E9MMX7Ad2cmFIUkCi9eNXMsa7OyWHHfoZHaM6kyjnDeI+g+Q5H/Ka3YGVLGtQEMTyXY8r59IR3bNmP05nrphmkb1U32Gx+eumCgS20Hu4FqSwUU6F245398KerVGKbAgGWBvQODejlkMlQhmJ7UXd4bEdl7XfIHVhWMLgWjjqqjoEqNKkaSBdcWIQwycMKk1zv/IXVsPtSuioYPBODFAUMOhSoVwiUmqiQa3; csm-hit=tb:s-YPH28TRMS0BFM670E3TE|1667224481904&t:1667224483220&adb:adblk_no; session-id-time=2082787201l");
//            call.Request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24\"");
//            call.Request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
//        }

//        Console.WriteLine($"Calling {call.Request.Url}");
//    };

//    //settings.AfterCall(call =>
//    //{
//    //    Console.WriteLine($"Called {call.Request.Url}");
//    //});
//    //settings.OnError(call =>
//    //{
//    //    Console.WriteLine($"Call to {call.Request.Url} failed: {call.Exception}");
//    //});
//});
FlurlHttp.ConfigureClient("https://www.amazon.com.tr", (settings) =>
{
    settings.BeforeCall(call =>
    {
        call.Request.CookieJar?.Clear();
        call.Request.Headers.Clear();
        call.Request.Headers.Add("cookie", "session-id=262-4776681-6994001; ubid-acbtr=259-0639309-9668916; lc-acbtr=tr_TR; check=true; x-acbtr=\"9Ky77cpaJZedv01QCuIV@i64OwblLJQ5RLiQzX9KBP5P3UGRqq2cA@xauVglzNwB\"; at-acbtr=Atza|IwEBICY2p6hpyXoCAvm74-aBXBg8GX_ySUjv0kzKrminXCSeBND3KW3-BaQXoyBvFr1LTFzCey65VXndtTpPNqvktEAsc7I5--yLidcxDCH-pJLUfEpb2zlQUBOWwVRGAkTABau07_M2UutmwYoUT-gJnqsduQl6NPiH0o5JSVh1L6nEBznsSKbiu15RsdRwdRnNFGkKwnv_F4_FpF0r-BKa7FQB; sess-at-acbtr=\"EdxvDxQWb+0KL8dhs5ejXcGCSwzvhtNKU2ZEjUxW+RQ=\"; sst-acbtr=Sst1|PQEhEr5OpbbfwFA_r_8022EoCSNtj5OJmScS9Yzbs8zmopzG6Vu0rRm0CDg3w3aDLysTtbm5g4JoenEjR84nFoIjHcWHCn9N_U0RBN15b7BqiqWLyt7OGfDDTYgwlPh9z6bENNkNVb-u7ugZH0E9OJJonDff_lwV1ZkEXgzhAhMVo27uuoMjZzMsEHA3mgbSorRY8jxmiFQRSMNWGhzpH-71BRLDqfT2o77duR8-b2PT7Dyua05JsqdpxwH-aBJDUbuiVqEw_5kiKzHObt8u1GBlH3yyc6Bo_Ju0dwaO79UjnEE; i18n-prefs=TRY; session-token=uBAN9RkDx83qQd/DH48K/A74zgw8E9MMX7Ad2cmFIUkCi9eNXMsa7OyWHHfoZHaM6kyjnDeI+g+Q5H/Ka3YGVLGtQEMTyXY8r59IR3bNmP05nrphmkb1U32Gx+eumCgS20Hu4FqSwUU6F245398KerVGKbAgGWBvQODejlkMlQhmJ7UXd4bEdl7XfIHVhWMLgWjjqqjoEqNKkaSBdcWIQwycMKk1zv/IXVsPtSuioYPBODFAUMOhSoVwiUmqiQa3; csm-hit=tb:s-YPH28TRMS0BFM670E3TE|1667224481904&t:1667224483220&adb:adblk_no; session-id-time=2082787201l");
        call.Request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24\"");
        call.Request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
        Console.WriteLine($"Calling {call.Request.Url}");
    });
    settings.AfterCall(call =>
    {
        Console.WriteLine($"Called {call.Request.Url}");
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
    });
    settings.AfterCall(call =>
    {
        Console.WriteLine($"Called {call.Request.Url}");
    });
    settings.OnError(call =>
    {
        Console.WriteLine($"Call to {call.Request.Url} failed: {call.Exception}");
    });
});
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