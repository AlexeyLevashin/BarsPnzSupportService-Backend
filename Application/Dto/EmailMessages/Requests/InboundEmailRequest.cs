namespace Application.Dto.EmailMessages.Requests;

public class InboundEmailRequest
{
    public long UidValidity { get; set; }
    public long Uid { get; set; }
    public string FromEmail { get; set; }
    public string? FromName { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string? InReplyTo { get; set; }
    public string? MessageId { get; set; }
    public List<string> References { get; set; } = new();
    public List<InboundEmailAttachment> Attachments { get; set; } = new();
}
