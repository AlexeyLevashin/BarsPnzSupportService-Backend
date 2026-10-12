namespace Application.Dto.EmailMessages.Requests;

public class EmailMessageRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public bool IsPlainText { get; set; }
    public List<EmailAttachmentRequest> Attachments { get; set; } = new();

    /// <summary>
    /// If set, MailKit adds Message-ID / In-Reply-To / References so all emails for this request form one thread.
    /// </summary>
    public Guid? ThreadRequestId { get; set; }
    public string? ReplyToMessageId { get; set; }
}
