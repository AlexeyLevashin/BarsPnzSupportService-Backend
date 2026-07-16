using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Employees;

public class EmployeeNotFoundException(string? message = "Сотрудник не найден в системе") : NotFoundException(message);