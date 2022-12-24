﻿using MimeKit;

namespace WebScraper.Api.V2.Business.Email;

public class MailMessage
{
    public List<MailboxAddress> To { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }

    public MailMessage(IEnumerable<string> to, string subject, string content)
    {
        To = new List<MailboxAddress>();
        To.AddRange(to.Select(x => new MailboxAddress(x, x)));
        Subject = subject;
        Content = content;
    }
}