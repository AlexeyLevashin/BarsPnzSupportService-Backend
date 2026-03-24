using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Institutions;

public class InstitutionHasEmployeesException(string? message = "Нельзя удалить учреждение, в котором числятся сотрудники. Сначала переведите или удалите персонал") : ConflictException(message);