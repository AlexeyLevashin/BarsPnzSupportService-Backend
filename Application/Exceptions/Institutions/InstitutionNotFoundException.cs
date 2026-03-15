using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Institutions;

public class InstitutionNotFoundException(string? message = "Учреждение не найдено в системе") : NotFoundException(message);