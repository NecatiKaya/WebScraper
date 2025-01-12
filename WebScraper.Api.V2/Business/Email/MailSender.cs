﻿using MailKit.Net.Smtp;
using MimeKit;

namespace WebScraper.Api.V2.Business.Email;

public class MailSender : IMailSender
{
    private readonly MailConfiguration _emailConfig;

    public MailSender(MailConfiguration emailConfig)
    {
        _emailConfig = emailConfig;
    }

    public async Task SendEmail(MailMessage message)
    {
        var emailMessage = CreateEmailMessage(message);
        await Send(emailMessage);
    }

    private MimeMessage CreateEmailMessage(MailMessage message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_emailConfig.From, _emailConfig.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content };
        return emailMessage;
    }

    private async Task Send(MimeMessage mailMessage)
    {
        using (var client = new SmtpClient())
        {
            try
            {
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                await client.SendAsync(mailMessage);
            }
            catch
            {
                //log an error message or throw an exception or both.
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }
    }
}