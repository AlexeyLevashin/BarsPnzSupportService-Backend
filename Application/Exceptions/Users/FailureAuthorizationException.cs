using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Users;

public class FailureAuthorizationException(string? message = "Неверные логин или пароль") : UnauthorizedException(message);