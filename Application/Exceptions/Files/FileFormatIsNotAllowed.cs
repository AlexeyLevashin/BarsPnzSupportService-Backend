using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Files;

public class FileFormatIsNotAllowed(string? message = "Нельзя приложить файл данного формата") : BadRequestException(message);