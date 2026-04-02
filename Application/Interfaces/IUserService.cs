using Application.Common.Pagination;
using Application.Dto.Institutions.Requests;
using Application.Dto.Users.Requests;
using Application.Dto.Users.Responses;
using Domain.DbModels;
using Domain.Enums;

namespace Application.Interfaces;

public interface IUserService
{
    public Task<GetUserResponse> GetMeAsync(Guid? id);
    public Task<CreateUserResponse> AddAsync(CreateUserByAdminRequest request, Guid userId, UserRole userRole, Guid? institutionId);
    public Task<GetUserResponse> GetUserById(Guid? userId);
    public Task<PagedResponse<GetUserResponse>> GetAllUsers(int pageNumber, int pageSize, UserRole userRole,  Guid? institutionId);
    public Task<GetUserResponse> UpdateAsync(CreateUserByAdminRequest request, Guid userId, Guid id, UserRole userRole, Guid? institutionId);
    public Task UpdatePasswordAsync(UpdateUserPasswordRequest request, Guid userId);
    public Task<CreateUserResponse> ForceResetPasswordAsync(Guid userId, Guid id, UserRole userRole, Guid? institutionId);
    public Task DeleteAsync(Guid userId, Guid id, UserRole userRole, Guid? institutionId);
}