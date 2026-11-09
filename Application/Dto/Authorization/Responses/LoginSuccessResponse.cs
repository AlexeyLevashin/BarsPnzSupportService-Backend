namespace Application.Dto.Authorization.Responses;

public class LoginSuccessResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }

    // Заполняется, если у пользователя включена двухфакторная аутентификация:
    // выдаётся токен подтверждения вместо полного доступа
    public bool RequiresTwoFactor { get; set; }
    public string? TwoFactorToken { get; set; }
}