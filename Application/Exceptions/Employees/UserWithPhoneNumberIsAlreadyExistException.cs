using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Employees;

public class EmployeeWithPhoneNumberIsAlreadyExistException(string? message = "Сотрдуник с данным номером телефона уже существует в системе") : ConflictException(message);