using Application.Common.Validators.Interfaces;
using Application.Dto.Employees.Requests;
using Application.Exceptions.Institutions;
using Application.Exceptions.JobTitles;
using Domain.Interfaces;

namespace Application.Common.Validators;

public class WorkplaceValidationService : IWorkplaceValidationService
{
    private readonly IInstitutionRepository _institutionRepository;
    private readonly IJobTitleRepository _jobTitleRepository;

    public WorkplaceValidationService(IInstitutionRepository institutionRepository, IJobTitleRepository jobTitleRepository)
    {
        _institutionRepository = institutionRepository;
        _jobTitleRepository = jobTitleRepository;
    }

    public async Task ValidateAsync(List<EmployeeInstitutionRequest> workplaces)
    {
        if (!workplaces.Any())
        {
            return;
        }

        var requestedInstitutionIds = workplaces
            .Select(w => w.InstitutionId)
            .Distinct()
            .ToList();

        var existingInstitutionsCount = await _institutionRepository.GetCountByIdsAsync(requestedInstitutionIds);
        if (existingInstitutionsCount != requestedInstitutionIds.Count)
        {
            throw new InstitutionNotFoundException();
        }

        var requestedJobTitleIds = workplaces
            .Select(w => w.JobTitleId)
            .OfType<Guid>()
            .Distinct()
            .ToList();

        if (requestedJobTitleIds.Any())
        {
            var existingJobTitlesCount = await _jobTitleRepository.GetCountByIdsAsync(requestedJobTitleIds);
            if (existingJobTitlesCount != requestedJobTitleIds.Count)
            {
                throw new JobTitleNotFoundException();
            }
        }
    }
}