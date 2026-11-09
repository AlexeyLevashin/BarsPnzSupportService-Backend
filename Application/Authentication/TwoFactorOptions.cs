namespace Application.Authentication;

public class TwoFactorOptions
{
    // Название сервиса в приложении-аутентификаторе (issuer в otpauth-URI)
    public string Issuer { get; set; } = "BarsPnz";

    // Количество одноразовых кодов восстановления, выдаваемых при включении
    public int RecoveryCodesCount { get; set; } = 8;

    // Неверных попыток ввода до блокировки
    public int MaxFailedAttempts { get; set; } = 5;

    // Длительность блокировки в минутах
    public int LockoutMinutes { get; set; } = 15;
}
