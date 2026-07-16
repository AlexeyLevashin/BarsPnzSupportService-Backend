using Application.Common.Pagination;
using Application.Dto.Institutions.Requests;
using Application.Dto.Institutions.Responses;
using Domain.DbModels;
using Domain.Enums;

namespace Application.Interfaces;

public interface IInstitutionService
{
    public Task<List<GetInstitutionResponse>> GetMy(Guid id);
    public Task<CreateInstitutionResponse> AddAsync(CreateInstitutionRequest request);
    public Task<GetInstitutionResponse> GetByIdAsync(Guid? id);
    public Task<PagedResponse<GetInstitutionResponse>> GetAllAsync(int pageNumber, int pageSize);
    public Task<GetInstitutionResponse> UpdateAsync(CreateInstitutionRequest request, Guid id);
    public Task DeleteAsync(Guid id);
}