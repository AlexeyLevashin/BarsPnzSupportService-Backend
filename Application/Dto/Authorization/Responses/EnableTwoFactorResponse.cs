namespace Application.Dto.Authorization.Responses;

public class EnableTwoFactorResponse
{
    public string Secret { get; set; }
    public string OtpAuthUri { get; set; }
}
