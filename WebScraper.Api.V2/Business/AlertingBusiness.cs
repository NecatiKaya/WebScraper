using System.Text;
using WebScraper.Api.V2.Business.Email;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Repositories;

namespace WebScraper.Api.V2.Business;

public class AlertingBusiness
{
    private readonly WebScraperDbContext _dbContext;

    private readonly IMailSender _mailSender;

    public AlertingBusiness(WebScraperDbContext dbContext, IMailSender mailSender)
    {
        _dbContext = dbContext;
        _mailSender = mailSender;
    }

    public async Task SendPriceAlertEmail()
    {
        ScraperVisitRepository repository = new ScraperVisitRepository(_dbContext);
        var items = await repository.GetNotNotifiedVisits();

        string rowTemplateFileName = "PriceItemTemplate.txt";
        string rowFilepath = Path.Combine(Environment.CurrentDirectory, @"Assets\EmailTemplates\", rowTemplateFileName);
        string rowFileContent = System.IO.File.ReadAllText(rowFilepath, System.Text.Encoding.UTF8);

        string htlmTemplateFileName = "PriceAlertTemplate.txt";
        string hmtlFilepath = Path.Combine(Environment.CurrentDirectory, @"Assets\EmailTemplates\", htlmTemplateFileName);
        string htmlFileContent = System.IO.File.ReadAllText(hmtlFilepath, System.Text.Encoding.UTF8);

        StringBuilder rowTemplateBuilder = new StringBuilder();
        items.ForEach((info) => {
            string rowTemplate = rowFileContent
            .Replace("##id", info.ProductId.ToString())
            .Replace("##name", info.ProductName)
            .Replace("##amazonPreviousPrice", info.AmazonPreviousPrice.ToString())
            .Replace("##amazonCurrentPrice", info.AmazonCurrentPrice.ToString())
            .Replace("##trendyolPreviousPrice", info.TrendyolPreviousPrice.ToString())
            .Replace("##trendyolCurrentPrice", info.TrendyolCurrentPrice.ToString())
            .Replace("##calculatedDifferenceAsAmount", info.CalculatedPriceDifferenceAsAmount.ToString())
            .Replace("##calculatedDifferenceAsPercentage", info.CalculatedPriceDifferenceAsPercentage.ToString())
            .Replace("##requestedDifferenceAsAmount", info.RequestedPriceDifferenceAsAmount.ToString())
            .Replace("##requestedDifferenceAsPercentage", info.RequestedPriceDifferenceAsPercentage.ToString());

            rowTemplateBuilder.Append(rowTemplate);
        });

        string rowHtml = rowTemplateBuilder.ToString();
        htmlFileContent = htmlFileContent.Replace("##rowTemplate", rowHtml);

        var message = new MailMessage(new string[] { "necatikaya86@hotmail.com", "fmuratkaya61@gmail.com" }, "Prices Are Changing - Good Luck", htmlFileContent);
        await _mailSender.SendEmail(message);

        int[] visitIds = items.Select(i => i.Id).ToArray();

        await repository.VisitsNotifiedAsync(visitIds);
    }
}