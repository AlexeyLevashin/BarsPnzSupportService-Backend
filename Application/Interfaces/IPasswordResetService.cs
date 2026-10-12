using Application.Dto.Authorization.Requests;
using Application.Dto.Authorization.Responses;

namespace Application.Interfaces;

public interface IPasswordResetService
{
    public Task RequestAsync(PasswordResetRequest request);
    public Task<VerifyPasswordResetResponse> VerifyAsync(VerifyPasswordResetRequest request);
    public Task ConfirmAsync(ConfirmPasswordResetRequest request);
}
