using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class UnacceptableStatusException(string? message = "Нельзя закрыть заявку с выбранным статусом") : ConflictException(message);