using System.ComponentModel.DataAnnotations;

namespace Application.Dto.PasswordResetCode.Requests;

public class PasswordResetEmailRequest
{
    [Required(ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }
}