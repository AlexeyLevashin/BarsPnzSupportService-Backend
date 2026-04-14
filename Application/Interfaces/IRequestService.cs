using Application.Common.Pagination;
using Application.Dto.Requests.Requests;
using Application.Dto.Requests.Responses;
using Domain.DbModels;

namespace Application.Interfaces;

public interface IRequestService
{
    public Task<CreateRequestResponse> AddAsync(CreateRequestRequest request, Guid userId);
    public Task<PagedResponse<GetRequestResponse>> GetAllAsync(int pageNumber, int pageSize);
    public Task<PagedResponse<GetRequestResponse>> GetMyAsync(int pageNumber, int pageSize, Guid? userId);
    public Task<GetRequestResponse> AssignToOperatorAsync(Guid? requestId, Guid operatorId);
}