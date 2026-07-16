using Application.Dto.JobTitles.Requests;
using Application.Dto.JobTitles.Responses;

namespace Application.Interfaces;

public interface IJobTitleService
{
    public Task<List<GetJobTitleResponse>> GetAllAsync();
    public Task<GetJobTitleResponse> AddAsync(CreateJobTitleRequest request);
}
