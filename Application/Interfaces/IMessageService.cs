using Application.Common.Pagination;
using Application.Dto.Messages.Requests;
using Application.Dto.Messages.Responses;
using Domain.DbModels;
using Domain.Enums;

namespace Application.Interfaces;

public interface IMessageService
{
    public Task<GetMessageResponse> AddAsync(Guid requestId, CreateMessageRequest request, Guid senderId, UserRole userRole);
    public Task<PagedResponse<GetMessageResponse>> GetAllMessagesAsync(int pageNumber, int pageSize, Guid? requestId);
    public Task<PagedResponse<GetMessageResponse>> GetAllCommentsAsync(int pageNumber, int pageSize, Guid? requestId);
}