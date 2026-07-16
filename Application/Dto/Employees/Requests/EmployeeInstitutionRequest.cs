using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Employees.Requests;

public class EmployeeInstitutionRequest
{
    [Required]
    public Guid InstitutionId { get; set; }
    public Guid? JobTitleId { get; set; }
}