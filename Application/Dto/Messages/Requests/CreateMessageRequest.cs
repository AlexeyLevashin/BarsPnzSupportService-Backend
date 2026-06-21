using System.ComponentModel.DataAnnotations;
using Domain.DbModels;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using MessageType = Domain.Enums.MessageType;

namespace Application.Dto.Messages.Requests;

public class CreateMessageRequest
{
    public string? Text { get; set; }
    public MessageType Type { get; set; }
    [EnumDataType(typeof(RequestStatus), ErrorMessage = "Недопустимый статус")]
    public RequestStatus Status { get; set; }
    public List<Guid>? AttachmentIds { get; set; } = new();
}