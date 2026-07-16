using Application.Dto.JobTitles.Requests;
using Application.Dto.JobTitles.Responses;
using Application.Exceptions.JobTitles;
using Application.Interfaces;
using Domain.DbModels;
using Domain.Interfaces;
using Mapster;

namespace Application.Services;

public class JobTitleService : IJobTitleService
{
    private readonly IJobTitleRepository _jobTitleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public JobTitleService(IJobTitleRepository jobTitleRepository, IUnitOfWork unitOfWork)
    {
        _jobTitleRepository = jobTitleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GetJobTitleResponse>> GetAllAsync()
    {
        var jobTitles = await _jobTitleRepository.GetAllAsync();
        return jobTitles.Adapt<List<GetJobTitleResponse>>();
    }

    public async Task<GetJobTitleResponse> AddAsync(CreateJobTitleRequest request)
    {
        var existing = await _jobTitleRepository.GetByNameAsync(request.Name.Trim());
        if (existing is not null)
        {
            throw new JobTitleIsAlreadyExistException();
        }

        var jobTitle = new DbJobTitle { Name = request.Name.Trim() };
        await _jobTitleRepository.AddAsync(jobTitle);
        await _unitOfWork.SaveChangesAsync();

        return jobTitle.Adapt<GetJobTitleResponse>();
    }
}
