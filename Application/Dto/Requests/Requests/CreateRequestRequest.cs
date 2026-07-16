using System.ComponentModel.DataAnnotations;
using Application.Dto.Messages.Requests;
using Domain.DbModels;
using Domain.Enums;

namespace Application.Dto.Requests.Requests;

public class CreateRequestRequest
{
    [Required(ErrorMessage = "Тема обязательна для сообщения")]
    public string Theme { get; set; }
    
    [Required(ErrorMessage = "Приоритет обязателен к выбору")]
    [EnumDataType(typeof(Priority), ErrorMessage = "Указан недопустимый приоритет")]
    public Priority Priority { get; set; }
    
    [Required(ErrorMessage = "Сообщение обязательно для заполнения")]
    public CreateFirstMessageRequest Message { get; set; }
    
    public Guid? InstitutionId { get; set; }
}