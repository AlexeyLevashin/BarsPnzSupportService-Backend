using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Dto.Authorization.Requests;

public class RegisterRequest //todo добавить полей для сотрудника и поменять сам метод
{
    [Required(ErrorMessage = "Имя обязательно для заполнения.")]
    [MinLength(1, ErrorMessage = "Имя не может быть пустым.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна для заполнения.")]
    [MinLength(1, ErrorMessage = "Фамилия не может быть пустой.")]
    public string Surname { get; set; }
    
    [MinLength(1, ErrorMessage = "Отчество не может быть пустым")]
    public string Patronymic { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Пароль обязателен для заполнения")]
    [MinLength(6, ErrorMessage = "Длина пароля должна быть не меньше 6 символов")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")] 
    public string ConfirmPassword { get; set; }
    
    public Guid? InstitutionId { get; set; }
}