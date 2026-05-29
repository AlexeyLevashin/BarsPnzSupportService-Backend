using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Attachments;

public class AttachmentNotFoundException(string? message = "Приложенный файл не найден") : NotFoundException(message);