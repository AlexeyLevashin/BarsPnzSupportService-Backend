using Domain.DbModels;
using Microsoft.AspNetCore.Http;
using MessageType = Domain.Enums.MessageType;

namespace Application.Dto.Messages.Requests;

public class CreateMessageRequest
{
    public string? Text { get; set; }
    public MessageType Type { get; set; }
    public List<Guid>? AttachmentIds { get; set; } = new();
}