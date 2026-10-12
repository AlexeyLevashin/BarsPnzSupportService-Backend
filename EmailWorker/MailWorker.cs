using Application.Dto.EmailMessages.Requests;
using Application.Exceptions.Abstractions;
using Application.Interfaces;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailWorker;

public class MailWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ImapOptions _options;
    private readonly ILogger<MailWorker> _logger;

    public MailWorker(IServiceScopeFactory scopeFactory, IOptions<ImapOptions> options, ILogger<MailWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.Username) ||
            string.IsNullOrWhiteSpace(_options.Password))
        {
            throw new InvalidOperationException("Настройте ImapSettings перед запуском EmailWorker");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var client = new ImapClient();
                _logger.LogInformation("Подключение к IMAP {Host}:{Port}", _options.Host, _options.Port);
                await client.ConnectAsync(_options.Host, _options.Port,
                    _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, stoppingToken);
                await client.AuthenticateAsync(_options.Username, _options.Password, stoppingToken);
                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite, stoppingToken);
                _logger.LogInformation("Подключено к IMAP {Host}; ожидание писем в INBOX", _options.Host);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessUnreadAsync(inbox, stoppingToken);

                    if (client.Capabilities.HasFlag(ImapCapabilities.Idle))
                    {
                        using var done = new CancellationTokenSource(TimeSpan.FromMinutes(9));
                        EventHandler<EventArgs> handler = (_, _) => done.Cancel();
                        inbox.CountChanged += handler;
                        try
                        {
                            await client.IdleAsync(done.Token, stoppingToken);
                        }
                        catch (OperationCanceledException) when (done.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
                        {
                            // Периодически выходим из IDLE и повторно ищем непрочитанные письма.
                        }
                        finally
                        {
                            inbox.CountChanged -= handler;
                        }
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    }
                }

                await client.DisconnectAsync(true, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ошибка подключения к почте; повтор через 30 секунд");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task ProcessUnreadAsync(IMailFolder inbox, CancellationToken cancellationToken)
    {
        var uids = await inbox.SearchAsync(SearchQuery.NotSeen, cancellationToken);
        _logger.LogInformation("Непрочитанных писем в INBOX: {Count}", uids.Count);
        foreach (var uid in uids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var message = await inbox.GetMessageAsync(uid, cancellationToken);
                var sender = message.From.Mailboxes.FirstOrDefault();
                if (string.Equals(sender?.Address, _options.Username, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(message.Headers["X-BarsPnz-Generated"], "true", StringComparison.OrdinalIgnoreCase))
                {
                    await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
                    _logger.LogInformation("Пропущено служебное письмо IMAP UID {Uid}", uid.Id);
                    continue;
                }

                var request = new InboundEmailRequest
                {
                    UidValidity = inbox.UidValidity,
                    Uid = uid.Id,
                    FromEmail = sender?.Address ?? string.Empty,
                    FromName = sender?.Name,
                    Subject = message.Subject ?? string.Empty,
                    Body = message.TextBody ?? message.HtmlBody ?? string.Empty,
                    InReplyTo = message.InReplyTo,
                    MessageId = message.MessageId,
                    References = message.References.ToList(),
                    Attachments = message.Attachments.Select(CreateAttachment).ToList()
                };

                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IInboundEmailService>();
                await service.ProcessAsync(request, cancellationToken);
                await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
            }
            catch (ForbiddenException exception) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(exception, "Письмо IMAP UID {Uid} не имеет доступа к заявке; помечено для ручной проверки", uid.Id);
                await inbox.AddFlagsAsync(uid, MessageFlags.Seen | MessageFlags.Flagged, true, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(exception, "Не удалось обработать письмо IMAP UID {Uid}", uid.Id);
            }
        }
    }

    private static InboundEmailAttachment CreateAttachment(MimeEntity entity)
    {
        var fileName = Path.GetFileName(entity.ContentDisposition?.FileName ?? entity.ContentType.Name ?? "attachment");
        var contentType = entity.ContentType.MimeType;
        if (contentType == "application/octet-stream")
            contentType = MimeTypes.GetMimeType(fileName);

        return new InboundEmailAttachment
        {
            FileName = fileName,
            ContentType = contentType,
            WriteContent = stream =>
            {
                switch (entity)
                {
                    case MimePart part:
                        part.Content.DecodeTo(stream);
                        break;
                    case MessagePart part:
                        part.Message.WriteTo(stream);
                        break;
                }
            }
        };
    }
}
