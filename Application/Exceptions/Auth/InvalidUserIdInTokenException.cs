using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Auth;

public class InvalidUserIdInTokenException(string? message = "Ошибка чтения токена: неверный формат ID пользователя") : UnauthorizedException(message);