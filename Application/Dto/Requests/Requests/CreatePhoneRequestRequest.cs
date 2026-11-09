using System.ComponentModel.DataAnnotations;
using Application.Dto.Messages.Requests;
using Domain.Enums;

namespace Application.Dto.Requests.Requests;

public class CreatePhoneRequestRequest
{
    [Required(ErrorMessage = "Клиент обязателен для выбора")]
    public Guid ClientId { get; set; }

    [Required(ErrorMessage = "Тема обязательна для сообщения")]
    public string Theme { get; set; }

    [Required(ErrorMessage = "Приоритет обязателен к выбору")]
    [EnumDataType(typeof(Priority), ErrorMessage = "Указан недопустимый приоритет")]
    public Priority Priority { get; set; }

    public Guid? InstitutionId { get; set; }

    [Required(ErrorMessage = "Описание звонка обязательно для заполнения")]
    public CreateFirstMessageRequest Message { get; set; }
}
