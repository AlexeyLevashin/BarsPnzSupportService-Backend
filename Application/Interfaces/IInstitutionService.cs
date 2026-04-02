using Application.Common.Pagination;
using Application.Dto.Institutions.Requests;
using Application.Dto.Institutions.Responses;
using Domain.DbModels;
using Domain.Enums;

namespace Application.Interfaces;

public interface IInstitutionService
{
    public Task<GetInstitutionResponse> GetMy(Guid? institutionsId);
    public Task<GetInstitutionResponse> AddAsync(CreateInstitutionRequest createInstitutionDto);
    public Task<GetInstitutionResponse> GetByIdAsync(Guid? id);
    public Task<PagedResponse<GetInstitutionResponse>> GetAllAsync(int pageNumber, int pageSize);
    public Task<GetInstitutionResponse> UpdateAsync(CreateInstitutionRequest updateInstitutionDto, Guid id);
    public Task DeleteAsync(Guid id);
}