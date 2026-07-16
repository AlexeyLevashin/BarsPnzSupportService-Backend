using Domain.DbModels;

namespace Domain.Interfaces;

public interface IJobTitleRepository
{
    public Task<int> GetCountByIdsAsync(List<Guid> jobTitleIds);
    public Task<List<DbJobTitle>> GetAllAsync();
    public Task<DbJobTitle?> GetByNameAsync(string name);
    public Task AddAsync(DbJobTitle jobTitle);
}