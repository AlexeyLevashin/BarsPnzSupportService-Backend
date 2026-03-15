using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Users;

public class UserNotFoundException(string? message = "Пользователь не найден в системе") : NotFoundException(message);