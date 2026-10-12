using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Authorization.Requests;

public class PasswordResetRequest
{
    [Required(ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }
}
