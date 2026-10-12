using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Authorization.Requests;

public class VerifyPasswordResetRequest : PasswordResetRequest
{
    [Required(ErrorMessage = "Код не может быть пустым")]
    [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Код должен состоять из 6 цифр")]
    public string Code { get; set; }
}
