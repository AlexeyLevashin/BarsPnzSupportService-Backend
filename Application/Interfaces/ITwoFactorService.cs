using Application.Dto.Authorization.Requests;
using Application.Dto.Authorization.Responses;

namespace Application.Interfaces;

public interface ITwoFactorService
{
    public Task<bool> IsEnabledAsync(Guid userId);
    public Task<TwoFactorStatusResponse> GetStatusAsync(Guid userId);
    public Task<EnableTwoFactorResponse> EnableAsync(Guid userId);
    public Task<ConfirmTwoFactorResponse> ConfirmAsync(Guid userId, ConfirmTwoFactorRequest request);
    public Task<LoginSuccessResponse> VerifyAsync(VerifyTwoFactorRequest request);
    public Task DisableAsync(Guid userId, ConfirmTwoFactorRequest request);
}
