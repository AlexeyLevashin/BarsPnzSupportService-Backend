namespace Application.Dto.Attachments.Responses;

public class AddAttachmentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string StorageKey { get; set; }
    public string ContentType { get; set; }
    public int FileSize { get; set; }
    public Guid? MessageId { get; set; }
}