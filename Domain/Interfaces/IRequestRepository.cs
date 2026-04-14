using Domain.DbModels;

namespace Domain.Interfaces;

public interface IRequestRepository
{
    public Task CreateAsync(DbRequest request);
    public Task<DbRequest?> GetByIdAsync(Guid? id);
    public Task<(List<DbRequest> Requests, int totalCount)> GetAllAsync(int pageNumber, int pageSize, Guid? userId = null);
    public void UpdateAsync(DbRequest dbRequest);

}