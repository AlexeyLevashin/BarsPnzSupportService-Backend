using Application.Exceptions.Abstractions;

namespace Application.Exceptions.JobTitles;

public class JobTitleIsAlreadyExistException(string? message = "Должность с таким названием уже существует") : ConflictException(message);
