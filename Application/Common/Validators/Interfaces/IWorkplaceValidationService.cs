using Application.Dto.Employees.Requests;

namespace Application.Common.Validators.Interfaces;

public interface IWorkplaceValidationService
{
    Task ValidateAsync(List<EmployeeInstitutionRequest> workplaces);
}