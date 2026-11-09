using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Authorization.Requests;

public class ConfirmTwoFactorRequest
{
    [Required(ErrorMessage = "Код обязателен для заполнения")]
    public string Code { get; set; }
}
