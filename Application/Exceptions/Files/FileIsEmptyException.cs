using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Files;

public class FileIsEmptyException(string? message = "Нельзя прикрепить пустой файл") : BadRequestException(message);