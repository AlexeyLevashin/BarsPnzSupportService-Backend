using Domain.Enums;

namespace Application.Dto.Users.Responses;

public class CreateUserResponse
{
    public string Email { get; set; }
    public string InitialPassword { get; set; }
}