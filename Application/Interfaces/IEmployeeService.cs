using Application.Common.Pagination;
using Application.Dto.Employees.Requests;
using Application.Dto.Employees.Responses;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IEmployeeService
{
    public Task<Guid> AddAsync(CreateEmployeeRequest request, UserRole role, List<Guid> institutionIds);
    public Task<Guid> UpdateAsync(Guid id, CreateEmployeeRequest request, UserRole userRole, List<Guid> institutionIds);
    public Task<PagedResponse<GetEmployeeResponse>> GetAllEmployeesAsync(int pageNumber, int pageSize, UserRole userRole, List<Guid> institutionIds);
    public Task DeleteAsync(Guid currentUserId, Guid employeeId, UserRole currentUserRole, List<Guid> institutionIds);
}