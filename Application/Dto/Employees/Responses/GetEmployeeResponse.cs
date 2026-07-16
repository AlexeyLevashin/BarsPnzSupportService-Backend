using Application.Dto.Users.Responses;

namespace Application.Dto.Employees.Responses;

public class GetEmployeeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Patronymic { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsUser { get; set; }
    public Guid? UserId { get; set; }
    public List<UserInstitutionResponse> Workplaces { get; set; } = new();
}