using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Dto.Requests.Requests;

public class UpdateRequestRequest
{
    [Required(ErrorMessage = "Тема обязательна для сообщения")]
    public string Theme { get; set; }
    
    [Required(ErrorMessage = "Приоритет обязателен к выбору")]
    [EnumDataType(typeof(Priority), ErrorMessage = "Указан недопустимый приоритет")]
    public Priority Priority { get; set; }

    public Guid? InstitutionId { get; set; }
}
