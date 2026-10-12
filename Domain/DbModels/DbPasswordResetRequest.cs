namespace Domain.DbModels;

public class DbPasswordResetRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string CodeHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int FailedAttempts { get; set; }
    public bool IsVerified { get; set; }
    public bool IsUsed { get; set; }
}
