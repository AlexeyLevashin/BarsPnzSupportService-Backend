using System.ComponentModel.DataAnnotations;
using Application.Dto.Employees.Requests;
using Domain.Enums;

namespace Application.Dto.UserWithEmployee.Requests;

public class CreateUserWithEmployeeRequest
{
    [Required(ErrorMessage = "Имя обязательно для заполнения.")]
    [MinLength(1, ErrorMessage = "Имя не может быть пустым.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна для заполнения.")]
    [MinLength(1, ErrorMessage = "Фамилия не может быть пустой.")]
    public string Surname { get; set; }
    
    public string? Patronymic { get; set; }
    
    [Phone(ErrorMessage = "Неверный формат номера телефона")]
    public string? PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Роль обязательна для выбора")]
    [EnumDataType(typeof(UserRole), ErrorMessage = "Указана недопустимая роль.")]
    public UserRole Role { get; set; }
    
    public List<EmployeeInstitutionRequest> Workplaces { get; set; } = new();
}