namespace WebScraper.Api.V2.Business.Email;

public interface IMailSender
{
    Task SendEmail(MailMessage message);
}
