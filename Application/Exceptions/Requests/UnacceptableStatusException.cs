using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class UnacceptableStatusException(string? message = "Текущий статус заявки не позволяет изменить статус на выбранный") : ConflictException(message);