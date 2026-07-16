using Application.Common.Pagination;
using Application.Dto.Institutions.Requests;
using Application.Dto.Users.Requests;
using Application.Dto.Users.Responses;
using Application.Dto.UserWithEmployee.Requests;
using Domain.DbModels;
using Domain.Enums;

namespace Application.Interfaces;

public interface IUserService
{
    public Task<GetUserResponse> GetMeAsync(Guid? id);
    public Task<CreateUserResponse> AddAsync(CreateUserByAdminRequest request,  Guid employeeId, UserRole userRole, List<Guid> institutionIds);
    public Task<CreateUserResponse> AddEmployeeWithUserAsync(CreateUserWithEmployeeRequest request, UserRole userRole, List<Guid> institutionIds);
    public Task<GetUserResponse> GetUserById(Guid? userId);
    public Task<GetUserResponse> GetUserByEmployeeIdAsync(Guid employeeId);
    public Task<PagedResponse<GetUserResponse>> GetAllUsersAsync(int pageNumber, int pageSize, UserRole userRole,  List<Guid> institutionIds);
    public Task<List<GetOperatorResponse>> GetSupervisorsAsync();
    public Task<GetUserResponse> UpdateAsync(CreateUserWithEmployeeRequest request, Guid userId, Guid id, UserRole userRole, List<Guid> institutionIds);
    public Task UpdatePasswordAsync(UpdateUserPasswordRequest request, Guid userId);
    public Task<CreateUserResponse> ForceResetPasswordAsync(Guid userId, Guid id, UserRole userRole,List<Guid> institutionIds);
    public Task RevoteAccessAsync(Guid userId, Guid id, UserRole userRole, List<Guid> institutionIds);
}