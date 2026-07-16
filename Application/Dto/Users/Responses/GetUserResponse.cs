using Domain.Enums;

namespace Application.Dto.Users.Responses;

public class GetUserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Patronymic { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public List<UserInstitutionResponse> Workplaces { get; set; } = new();
}