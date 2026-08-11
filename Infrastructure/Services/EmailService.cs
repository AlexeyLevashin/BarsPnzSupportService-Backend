using Application.Dto.EmailMessages.Requests;
using Application.Interfaces.Repositories;
using Infrastructure.Common;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpOptions _options;
    
    public EmailService(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }
    
    public async Task SendEmailAsync(EmailMessageRequest request)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_options.FromName, _options.Username));
        email.To.Add(new MailboxAddress("", request.ToEmail));
        email.Subject = request.Subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = request.Message
        };
        
        if (request.ThreadRequestId is { } requestId)
        {
            ApplyRequestThreadHeaders(email, requestId);
        }

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl);
            await client.AuthenticateAsync(_options.Username, _options.Password);
            await client.SendAsync(email);

            await client.DisconnectAsync(true);
        }
    }

    private void ApplyRequestThreadHeaders(MimeMessage email, Guid requestId)
    {
        var domain = GetMessageIdDomain();
        var rootId = $"request-{requestId}@{domain}";

        email.MessageId = $"request-{requestId}-{Guid.NewGuid():N}@{domain}";
        email.InReplyTo = rootId;
        email.References.Add(rootId);
    }

    private string GetMessageIdDomain()
    {
        var username = _options.Username;
        if (string.IsNullOrWhiteSpace(username))
        {
            return "localhost";
        }

        var at = username.LastIndexOf('@');
        if (at >= 0 && at < username.Length - 1)
        {
            return username[(at + 1)..];
        }

        return "localhost";
    }
}
