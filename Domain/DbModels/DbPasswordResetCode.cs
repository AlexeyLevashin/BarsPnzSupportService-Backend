namespace Domain.DbModels;

public class DbPasswordResetCode
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string CodeHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public Guid? ResetToken { get; set; }
    public DbUser User { get; set; }
}