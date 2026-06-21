using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class RequestIsClosedException(string? message = "Заявка закрыта, добавить сообщение невозможно") : ConflictException(message);