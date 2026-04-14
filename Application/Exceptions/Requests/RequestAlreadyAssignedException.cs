using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class RequestAlreadyAssignedException(string? message = "Данная заявка уже закреплена за другим оператором") : ConflictException(message);