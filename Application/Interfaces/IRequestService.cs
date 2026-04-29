using Application.Common.Pagination;
using Application.Dto.Messages.Responses;
using Application.Dto.Requests.Requests;
using Application.Dto.Requests.Responses;
using Domain.DbModels;

namespace Application.Interfaces;

public interface IRequestService
{
    public Task<CreateRequestResponse> AddAsync(CreateRequestRequest request, Guid userId);
    public Task<PagedResponse<GetRequestResponse>> GetAllAsync(int pageNumber, int pageSize);
    public Task<PagedResponse<GetRequestResponse>> GetMyAsync(int pageNumber, int pageSize, Guid? userId);
    public Task<GetRequestResponse> GetRequestByIdAsync(Guid? id);
    public Task AssignToOperatorAsync(Guid? requestId, Guid operatorId);
}