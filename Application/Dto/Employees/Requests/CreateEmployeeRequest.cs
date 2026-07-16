using System.ComponentModel.DataAnnotations;
using Domain.DbModels;

namespace Application.Dto.Employees.Requests;

public class CreateEmployeeRequest
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

    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string? Email { get; set; }
    
    public List<EmployeeInstitutionRequest> Workplaces { get; set; } = new();
}