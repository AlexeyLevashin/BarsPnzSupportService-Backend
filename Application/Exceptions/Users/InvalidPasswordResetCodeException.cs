using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Users;

public class InvalidPasswordResetCodeException(string? message = "Неверный или просроченный код") : BadRequestException(message);