using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Files;

public class MaxFileSizeException(string? message = "Превышен максимальный размер файла. Размер файла не должен превышать 200МБ") : BadRequestException(message);