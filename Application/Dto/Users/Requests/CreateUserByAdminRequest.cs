using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Dto.Users.Requests;

public class CreateUserByAdminRequest
{
    [Required(ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Роль обязательна для выбора")]
    [EnumDataType(typeof(UserRole), ErrorMessage = "Указана недопустимая роль.")]
    public UserRole Role { get; set; }
}