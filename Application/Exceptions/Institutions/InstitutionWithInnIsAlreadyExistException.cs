using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Institutions;

public class InstitutionWithInnIsAlreadyExistException(string? message = "Учреждение с данным номером ИНН уже существует в системе") : ConflictException(message);