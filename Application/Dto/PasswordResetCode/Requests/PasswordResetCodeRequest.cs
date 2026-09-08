using System.ComponentModel.DataAnnotations;

namespace Application.Dto.PasswordResetCode.Requests;

public class PasswordResetCodeRequest
{
    [Required(ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Код не может быть пустым")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Код должен состоять из 6 цифр")]
    public string Code { get; set; }
}