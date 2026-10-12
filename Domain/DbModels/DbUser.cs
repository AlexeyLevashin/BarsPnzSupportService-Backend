using Domain.Enums;

namespace Domain.DbModels;

public class DbUser
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public int PasswordResetAttemptsRemaining { get; set; } = 3;
    public DateTime? PasswordResetAttemptsRefillAt { get; set; }
    public UserRole Role { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsActivated { get; set; } = true;
    public Guid EmployeeId { get; set; }
    public DbEmployee Employee { get; set; }
    public List<DbRequest> Requests { get; set; } = new();
}
