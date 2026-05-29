using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Messages;

public class MessageNotFoundException(string? message = "Сообщение не найдено") : NotFoundException(message);