using Application.Dto.EmailMessages.Requests;

namespace Application.Interfaces;

public interface IEmailTemplateBuilder
{
    public EmailContent BuildNewUserCredentials(string email, string password);
    public EmailContent BuildNewRequestStatus(string theme);
}