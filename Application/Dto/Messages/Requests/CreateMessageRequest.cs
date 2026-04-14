using Domain.DbModels;
using Microsoft.AspNetCore.Http;
using Minio.DataModel.Select;
using MessageType = Domain.Enums.MessageType;

namespace Application.Dto.Messages.Requests;

public class CreateMessageRequest
{
    public string? Text { get; set; }
    public Guid RequestId { get; set; }
    public MessageType Type { get; set; }
    public List<IFormFile>? Files { get; set; } = new();
}