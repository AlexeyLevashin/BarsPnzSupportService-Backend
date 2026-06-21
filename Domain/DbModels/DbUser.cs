using Domain.Enums;

namespace Domain.DbModels;

public class DbUser
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Patronymic { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Guid? InstitutionId { get; set; }
    public List<DbRequest> Requests { get; set; } = new();
    public DbInstitution? Institution { get; set; }
}   