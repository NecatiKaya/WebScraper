namespace WebScraper.Api.Business.Email;

public interface IMailSender
{
    Task SendEmail(MailMessage message);
}
