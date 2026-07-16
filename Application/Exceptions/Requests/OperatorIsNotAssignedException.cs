using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class OperatorIsNotAssignedException(string? message = "Выбранный оператор не назначен на данную заявку") : ConflictException(message);