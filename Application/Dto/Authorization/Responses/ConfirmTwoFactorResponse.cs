namespace Application.Dto.Authorization.Responses;

public class ConfirmTwoFactorResponse
{
    public List<string> RecoveryCodes { get; set; } = new();
}
