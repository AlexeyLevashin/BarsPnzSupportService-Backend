namespace Domain.DbModels;

public class DbTwoFactorRecoveryCode
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TwoFactorAuthId { get; set; }
    public string CodeHash { get; set; }
    public bool IsUsed { get; set; }
    public DbTwoFactorAuth TwoFactorAuth { get; set; }
}
