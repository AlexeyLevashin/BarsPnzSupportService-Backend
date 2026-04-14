using Application.Dto.Messages.Requests;
using Application.Dto.Messages.Responses;
using Domain.DbModels;
using Domain.Enums;

namespace Application.Interfaces;

public interface IMessageService
{
    public Task<GetMessageResponse> AddAsync(CreateMessageRequest request, Guid senderId, UserRole userRole);
}