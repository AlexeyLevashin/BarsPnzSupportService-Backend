using Domain.DbModels;

namespace Domain.Interfaces;

public interface IInstitutionRepository
{
    public Task AddAsync(DbInstitution dbInstitution);
    public void UpdateAsync(DbInstitution dbInstitution);
    public void DeleteAsync(DbInstitution dbInstitution);
    public Task<DbInstitution?> GetByIdAsync(Guid id);
    public Task<List<DbInstitution>> GetAllByEmployeeIdAsync(Guid employeeId);
    public Task<int> GetCountByIdsAsync(List<Guid> institutionIds);
    public Task<DbInstitution?> GetByInnAsync(string inn);
    public Task<(List<DbInstitution> Institutions, int totalCount)> GetAllAsync(int pageNumber, int pageSize);
};
