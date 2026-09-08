using System.ComponentModel.DataAnnotations;

namespace Application.Dto.PasswordResetCode.Requests;

public class CompletePasswordResetRequest 
{
    [Required (ErrorMessage = "Токен не может быть пустым")]
    public Guid ResetToken { get; set; }
    
    [Required (ErrorMessage = "Пароль обязателен для заполнения")]
    [MinLength(6, ErrorMessage = "Длина пароля должна быть не меньше 6 символов")]
    public string NewPassword { get; set; }
    
    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")] 
    public string ConfirmNewPassword { get; set; }
}