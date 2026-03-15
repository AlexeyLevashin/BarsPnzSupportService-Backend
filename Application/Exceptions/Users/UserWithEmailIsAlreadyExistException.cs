using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Users;

public class UserWithEmailIsAlreadyExistException(string? message = "Пользователь с данным email уже существует в системе") : ConflictException(message);