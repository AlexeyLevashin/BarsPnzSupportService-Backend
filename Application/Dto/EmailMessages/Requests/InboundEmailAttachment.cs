namespace Application.Dto.EmailMessages.Requests;

public class InboundEmailAttachment
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public Action<Stream> WriteContent { get; set; }
}
