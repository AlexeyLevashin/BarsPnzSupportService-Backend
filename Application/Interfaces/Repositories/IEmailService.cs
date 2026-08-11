using Application.Dto.EmailMessages.Requests;

namespace Application.Interfaces.Repositories;

public interface IEmailService
{
    public Task SendEmailAsync(EmailMessageRequest request);
}