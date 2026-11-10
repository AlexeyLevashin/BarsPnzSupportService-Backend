namespace Application.Dto.Messages.Requests;

public class UpdateMessageRequest
{
    public string? Text { get; set; }
    public List<Guid>? AttachmentIds { get; set; } = new();
}