namespace Domain.Interfaces;

public interface IJobTitleRepository
{
    public Task<int> GetCountByIdsAsync(List<Guid> jobTitleIds);
}