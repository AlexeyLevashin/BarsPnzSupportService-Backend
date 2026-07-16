using Application.Dto.Employees.Requests;
using Domain.DbModels;

namespace Application.Extensions;

public static class EmployeeInstitutionExtensions
{
    public static void SyncEmployeeInstitutions(this List<DbEmployeeInstitution> existingInstitutions, List<EmployeeInstitutionRequest> newWorkplaces)
    {
        var newWorkplacesList = newWorkplaces.ToList();

        existingInstitutions.RemoveAll(ei => newWorkplacesList.All(nw => nw.InstitutionId != ei.InstitutionId));

        foreach (var existing in existingInstitutions)
        {
            existing.JobTitleId = newWorkplacesList
                .First(nw => nw.InstitutionId == existing.InstitutionId).JobTitleId;
        }

        var itemsToAdd = newWorkplacesList
            .Where(nw => existingInstitutions.All(ei => ei.InstitutionId != nw.InstitutionId))
            .Select(nw => new DbEmployeeInstitution 
            { 
                InstitutionId = nw.InstitutionId, 
                JobTitleId = nw.JobTitleId 
            });

        existingInstitutions.AddRange(itemsToAdd);
    }
}