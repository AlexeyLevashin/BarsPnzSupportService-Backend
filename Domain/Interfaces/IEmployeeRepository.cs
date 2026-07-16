using Domain.DbModels;

namespace Domain.Interfaces;

public interface IEmployeeRepository
{
    public Task AddAsync(DbEmployee employee);
    public void DeleteAsync(DbEmployee employee);
    public Task<bool> CheckByPhoneNumberExistsAsync(string? phoneNumber, Guid? employeeId = null);
    public Task<bool> CheckByEmailExistsAsync(string? email, Guid? employeeId = null);
    public Task<DbEmployee?> GetByIdAsync(Guid employeeId);
    public Task<(List<DbEmployee> Employees, int totalCount)> GetAllAsync(int pageNumber, int pageSize, List<Guid>? institutionIds);
}