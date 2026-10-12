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
    private readonly IMinioFileStorageService _storage;
    
    public EmailService(IOptions<SmtpOptions> options, IMinioFileStorageService storage)
    {
        _options = options.Value;
        _storage = storage;
    }
    
    public async Task SendEmailAsync(EmailMessageRequest request)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_options.FromName, _options.Username));
        email.To.Add(new MailboxAddress("", request.ToEmail));
        email.Subject = request.Subject;
        email.Headers.Add("X-BarsPnz-Generated", "true");
        var body = new BodyBuilder();
        if (request.IsPlainText)
            body.TextBody = request.Message;
        else
            body.HtmlBody = request.Message;

        foreach (var attachment in request.Attachments)
        {
            var content = await _storage.DownloadAsync(attachment.StorageKey, "attachments");
            body.Attachments.Add(attachment.FileName, content,
                ContentType.Parse(attachment.ContentType));
        }
        email.Body = body.ToMessageBody();
        
        if (request.ThreadRequestId is { } requestId)
        {
            ApplyRequestThreadHeaders(email, requestId, request.ReplyToMessageId);
        }

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl);
            await client.AuthenticateAsync(_options.Username, _options.Password);
            await client.SendAsync(email);

            await client.DisconnectAsync(true);
        }
    }

    private void ApplyRequestThreadHeaders(MimeMessage email, Guid requestId, string? replyToMessageId)
    {
        var domain = GetMessageIdDomain();
        var rootId = $"request-{requestId}@{domain}";

        email.MessageId = $"request-{requestId}-{Guid.NewGuid():N}@{domain}";
        email.InReplyTo = string.IsNullOrWhiteSpace(replyToMessageId) ? rootId : replyToMessageId;
        if (!string.IsNullOrWhiteSpace(replyToMessageId))
            email.References.Add(replyToMessageId);
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
