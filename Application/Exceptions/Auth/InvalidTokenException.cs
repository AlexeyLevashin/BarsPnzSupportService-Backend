using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Auth;

public class InvalidTokenException(string? message = "Невалидный или просроченный Refresh-токен") : UnauthorizedException(message);