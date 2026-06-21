using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Messages;

public class RequestForbiddenException(string? message = "Нельзя писать сообщения в другие заявки") : ForbiddenException(message);