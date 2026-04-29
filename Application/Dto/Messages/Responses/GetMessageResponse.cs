using Domain.Enums;

namespace Application.Dto.Messages.Responses;

public class GetMessageResponse
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string? Text { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public MessageType Type { get; set; }
    public string SenderFullName { get; set; }
    public Guid SenderId { get; set; }
    public Guid RequestId { get; set; }
}