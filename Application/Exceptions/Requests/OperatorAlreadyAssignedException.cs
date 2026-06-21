using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class OperatorAlreadyAssignedException(string? message = "Выбранный оператор уже назначен на данную заявку") : ConflictException(message);