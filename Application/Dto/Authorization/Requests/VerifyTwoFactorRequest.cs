using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Authorization.Requests;

public class VerifyTwoFactorRequest
{
    [Required(ErrorMessage = "Токен подтверждения обязателен")]
    public string TwoFactorToken { get; set; }

    [Required(ErrorMessage = "Код обязателен для заполнения")]
    public string Code { get; set; }
}
