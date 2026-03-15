using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class RequestNotFoundException(string? message = "Заявка не найдена в системе") : NotFoundException(message);