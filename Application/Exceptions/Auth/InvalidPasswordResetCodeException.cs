using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Auth;

public class InvalidPasswordResetCodeException(string? message = "Неверный или просроченный код восстановления") : BadRequestException(message);
