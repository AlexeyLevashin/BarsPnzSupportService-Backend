using System.Text.RegularExpressions;
using Application.Common.Minio;
using Application.Dto.EmailMessages.Requests;
using Application.Dto.Messages.Responses;
using Application.Dto.Requests.Responses;
using Application.Exceptions.Abstractions;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Interfaces.Repositories;
using Hangfire;

namespace Application.Services;

public class InboundEmailService : IInboundEmailService
{
    private static readonly Regex RequestIdPattern = new(@"request-(?<id>[0-9a-fA-F-]{36})(?:-[0-9a-fA-F]{32})?@",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex QuotedReplyPattern = new(
        @"^[ \t]*(?:(?:пн|вт|ср|чт|пт|сб|вс),[^\r\n]{0,500}\b\d{1,2}:\d{2}\b[^\r\n]*<[^<>\r\n]+@[^<>\r\n]+>:|On [^\r\n]{1,500} wrote:)[ \t]*\r?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private readonly IUserRepository _users;
    private readonly IEmployeeRepository _employees;
    private readonly IRequestRepository _requests;
    private readonly IMessageRepository _messages;
    private readonly IInboundEmailRepository _inboundEmails;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwords;
    private readonly IRequestNotificationService _notifications;
    private readonly ILogger<InboundEmailService> _logger;
    private readonly IMinioFileStorageService _storage;
    private readonly FileUploadOptions _fileOptions;
    private readonly IBackgroundJobClient _backgroundJobs;

    public InboundEmailService(IUserRepository users, IEmployeeRepository employees,
        IRequestRepository requests, IMessageRepository messages, IInboundEmailRepository inboundEmails,
        IUnitOfWork unitOfWork, IPasswordService passwords, IRequestNotificationService notifications,
        ILogger<InboundEmailService> logger, IMinioFileStorageService storage,
        IOptions<FileUploadOptions> fileOptions, IBackgroundJobClient backgroundJobs)
    {
        _users = users;
        _employees = employees;
        _requests = requests;
        _messages = messages;
        _inboundEmails = inboundEmails;
        _unitOfWork = unitOfWork;
        _passwords = passwords;
        _notifications = notifications;
        _logger = logger;
        _storage = storage;
        _fileOptions = fileOptions.Value;
        _backgroundJobs = backgroundJobs;
    }

    public async Task ProcessAsync(InboundEmailRequest email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email.FromEmail))
        {
            throw new ArgumentException("У письма отсутствует адрес отправителя");
        }

        Guid? requestId = null;
        Guid? messageId = null;
        var newRequest = false;

        if (await _inboundEmails.ExistsAsync(email.UidValidity, email.Uid, cancellationToken))
        {
            return;
        }

        var quotedReply = QuotedReplyPattern.Match(email.Body);
        if (quotedReply.Success)
            email.Body = email.Body[..quotedReply.Index].TrimEnd();

        var senderEmail = email.FromEmail.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(senderEmail);
        if (user is null)
        {
            if (await _users.IsEmailTakenAsync(senderEmail))
            {
                throw new InvalidOperationException("Письмо отправлено с адреса удаленной учетной записи");
            }

            var employee = await _employees.GetByEmailAsync(senderEmail);
            if (employee?.IsUser == true)
            {
                throw new InvalidOperationException("У сотрудника уже есть учетная запись");
            }

            employee ??= CreateEmployee(email, senderEmail);
            employee.IsUser = true;
            employee.Email = senderEmail;
            user = new DbUser
            {
                Email = senderEmail,
                Employee = employee,
                Role = UserRole.User,
                PasswordHash = _passwords.Hash(_passwords.GeneratePassword()),
                IsActivated = false
            };
            await _users.AddAsync(user);
        }

        if (user.Employee is null)
        {
            throw new InvalidOperationException("Для отправителя не найден действующий сотрудник");
        }

        var referencedId = await FindReferencedRequestIdAsync(email, cancellationToken);
        if (referencedId.HasValue)
        {
            var request = await _requests.GetByIdAsync(referencedId.Value);
            if (request is null || request.ClientId != user.Id &&
                !(user.Role == UserRole.SuperAdmin ||
                  user.Role == UserRole.Operator && await _requests.CheckAlreadyAssigned(request.Id, user.Id)))
            {
                throw new ForbiddenException("Отправитель не может отвечать на эту заявку");
            }

            if (request.Status != RequestStatus.New && request.Status != RequestStatus.InProgress)
            {
                request.Status = RequestStatus.InProgress;
                request.ClosedAt = null;
            }

            var reply = new DbMessage
            {
                RequestId = request.Id,
                SenderId = user.Id,
                Text = email.Body,
                Type = MessageType.Public
            };
            await AddAttachmentsAsync(reply, email, cancellationToken);
            await _messages.CreateAsync(reply);
            requestId = request.Id;
            messageId = reply.Id;
        }
        else
        {
            var request = new DbRequest
            {
                Theme = string.IsNullOrWhiteSpace(email.Subject) ? "Без темы" : email.Subject,
                ClientId = user.Id,
                Client = user,
                InstitutionId = user.Employee.EmployeeInstitutions.Count == 1
                    ? user.Employee.EmployeeInstitutions[0].InstitutionId : null,
                Status = RequestStatus.New,
                Type = RequestType.Email,
                Priority = Priority.Normal
            };
            var firstMessage = new DbMessage
            {
                SenderId = user.Id,
                Sender = user,
                Text = email.Body,
                Type = MessageType.Public
            };
            await AddAttachmentsAsync(firstMessage, email, cancellationToken);
            request.Messages.Add(firstMessage);
            await _requests.CreateAsync(request);
            requestId = request.Id;
            newRequest = true;
        }

        await _inboundEmails.AddAsync(new DbInboundEmail
        {
            UidValidity = email.UidValidity,
            Uid = email.Uid,
            MessageId = NormalizeMessageId(email.MessageId),
            RequestId = requestId.Value
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (newRequest)
        {
            try
            {
                var confirmation = new EmailMessageRequest
                {
                    ToEmail = senderEmail,
                    Subject = $"Заявка: {(string.IsNullOrWhiteSpace(email.Subject) ? "Без темы" : email.Subject)}",
                    Message = $"Ваше обращение зарегистрировано.\nНомер заявки: {requestId}.\nОтветьте на это письмо, чтобы дополнить заявку.",
                    IsPlainText = true,
                    ThreadRequestId = requestId,
                    ReplyToMessageId = NormalizeMessageId(email.MessageId)
                };
                _backgroundJobs.Enqueue<IEmailService>(x => x.SendEmailAsync(confirmation));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось поставить подтверждение заявки {RequestId} в очередь", requestId);
            }
        }

        try
        {
            if (newRequest)
            {
                var request = await _requests.GetByIdAsync(requestId.Value);
                if (request is not null)
                {
                    await _notifications.NotifyNewRequestAsync(request.Adapt<GetRequestResponse>());
                }
            }
            else if (messageId.HasValue)
            {
                var message = await _messages.GetByIdAsync(messageId.Value);
                if (message is not null)
                {
                    await _notifications.NotifyNewMessageAsync(requestId.Value, message.Adapt<GetMessageResponse>());
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось отправить уведомление по почтовой заявке {RequestId}", requestId);
        }
    }

    private async Task AddAttachmentsAsync(DbMessage message, InboundEmailRequest email,
        CancellationToken cancellationToken)
    {
        var skipped = new List<string>();
        var maxFileSize = (long)_fileOptions.MaxFileSizeMb * 1024 * 1024;
        for (var index = 0; index < email.Attachments.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var attachment = email.Attachments[index];
            if (!_fileOptions.AllowedContentTypes.Contains(attachment.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Вложение {FileName} пропущено: неподдерживаемый тип {ContentType}",
                    attachment.FileName, attachment.ContentType);
                skipped.Add(attachment.FileName);
                continue;
            }

            using var stream = new LimitedMemoryStream(maxFileSize);
            try
            {
                attachment.WriteContent(stream);
            }
            catch (AttachmentTooLargeException)
            {
                _logger.LogWarning("Вложение {FileName} пропущено: размер превышает {MaxFileSizeMb} МБ",
                    attachment.FileName, _fileOptions.MaxFileSizeMb);
                skipped.Add(attachment.FileName);
                continue;
            }

            if (stream.Length == 0)
            {
                _logger.LogWarning("Вложение {FileName} пропущено: пустой файл", attachment.FileName);
                skipped.Add(attachment.FileName);
                continue;
            }

            stream.Position = 0;
            var storageKey = $"email-{email.UidValidity}-{email.Uid}-{index}{Path.GetExtension(attachment.FileName)}";
            await _storage.UploadAsync(storageKey, "attachments", stream, attachment.ContentType);
            message.Attachments.Add(new DbAttachment
            {
                FileName = attachment.FileName,
                StorageKey = storageKey,
                ContentType = attachment.ContentType,
                FileSize = stream.Length
            });
        }

        if (skipped.Count > 0)
            message.Text = $"{email.Body}\n\nНе добавлены вложения: {string.Join(", ", skipped)}.";
    }

    private sealed class AttachmentTooLargeException : Exception { }

    private sealed class LimitedMemoryStream(long maxLength) : MemoryStream
    {
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckLength(count);
            base.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            CheckLength(buffer.Length);
            base.Write(buffer);
        }

        public override void WriteByte(byte value)
        {
            CheckLength(1);
            base.WriteByte(value);
        }

        private void CheckLength(int count)
        {
            if (count > maxLength - Position)
                throw new AttachmentTooLargeException();
        }
    }

    private async Task<Guid?> FindReferencedRequestIdAsync(InboundEmailRequest email,
        CancellationToken cancellationToken)
    {
        foreach (var reference in new[] { email.InReplyTo }.Concat(email.References))
        {
            var match = RequestIdPattern.Match(reference ?? string.Empty);
            if (match.Success && Guid.TryParse(match.Groups["id"].Value, out var requestId))
            {
                return requestId;
            }

            var messageId = NormalizeMessageId(reference);
            if (messageId is not null)
            {
                var originalRequestId = await _inboundEmails.GetRequestIdByMessageIdAsync(messageId, cancellationToken);
                if (originalRequestId.HasValue)
                    return originalRequestId;
            }
        }

        return null;
    }

    private static string? NormalizeMessageId(string? messageId)
    {
        var normalized = messageId?.Trim().Trim('<', '>');
        return string.IsNullOrWhiteSpace(normalized) || normalized.Length > 512 ? null : normalized;
    }

    private static DbEmployee CreateEmployee(InboundEmailRequest email, string address)
    {
        var parts = (email.FromName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new DbEmployee
        {
            Email = address,
            Surname = parts.Length > 1 ? parts[0] : null,
            Name = parts.Length > 1 ? parts[1] : parts.FirstOrDefault(),
            Patronymic = parts.Length > 2 ? parts[2] : null
        };
    }
}
