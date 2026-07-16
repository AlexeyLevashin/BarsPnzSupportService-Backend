using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Institutions.Requests;

public class CreateInstitutionRequest
{
    [Required(ErrorMessage = "Наименование учреждения обязательно для заполнения")]
    [MinLength(1, ErrorMessage = "Наименование учреждения не может быть пустым")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "ИНН обязателен для заполнения")]
    [RegularExpression(@"^(\d{10}|\d{12})$", ErrorMessage = "ИНН должен состоять из 10 или 12 цифр")]
    public string INN { get; set; }
    
    [Phone(ErrorMessage = "Неверный формат номера телефона")]
    public string? PhoneNumber { get; set; }
   
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string? Email { get; set; }
    
    public Guid? HeadId { get; set; }
}