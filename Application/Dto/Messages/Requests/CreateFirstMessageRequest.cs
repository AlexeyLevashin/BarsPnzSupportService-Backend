using Microsoft.AspNetCore.Http;

namespace Application.Dto.Messages.Requests;

public class CreateFirstMessageRequest
{
    public string? Text { get; set; }
    public List<Guid>? AttachmentIds { get; set; } = new();
}