namespace Domain.Interfaces;

public interface ITwoFactorCodeProvider
{
    public string GenerateSecretBase32();
    public string BuildOtpAuthUri(string issuerName, string accountName, string secretBase32);
    public bool VerifyCode(string secretBase32, string code, long lastUsedStep, out long usedStep, long? currentDateStep = null);
    public string GenerateRecoveryCode();
}
