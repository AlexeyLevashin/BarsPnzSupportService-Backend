namespace Application.Dto.Attachments.Requests;

public class AddAttachmentRequest
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string StorageKey { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
}