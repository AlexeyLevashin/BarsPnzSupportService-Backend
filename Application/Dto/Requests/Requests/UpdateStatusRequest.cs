using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Dto.Requests.Requests;

public class UpdateStatusRequest
{
    [Required] 
    [EnumDataType(typeof(RequestStatus), ErrorMessage = "Недопустимый статус")]
    public RequestStatus Status { get; set; }
}