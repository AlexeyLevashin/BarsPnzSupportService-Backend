using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Employees;

public class EmployeeWithEmailIsAlreadyExistException(string? message = "Сотрдуник с данным email уже существует в системе") : ConflictException(message);