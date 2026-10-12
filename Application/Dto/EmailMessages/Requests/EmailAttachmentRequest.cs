namespace Application.Dto.EmailMessages.Requests;

public class EmailAttachmentRequest
{
    public string FileName { get; set; }
    public string StorageKey { get; set; }
    public string ContentType { get; set; }
}
