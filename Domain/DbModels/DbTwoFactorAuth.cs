namespace Domain.DbModels;

public class DbTwoFactorAuth
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string Secret { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public long LastUsedStep { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public List<DbTwoFactorRecoveryCode> RecoveryCodes { get; set; } = new();
}
