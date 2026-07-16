using Application.Exceptions.Abstractions;

namespace Application.Exceptions.JobTitles;

public class JobTitleNotFoundException(string? message = "Должность не найдена в системе") : NotFoundException(message);