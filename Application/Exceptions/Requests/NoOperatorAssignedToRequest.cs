using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class NoOperatorAssignedToRequest(string? message = "Данное действие можно совершать только операторам, назначенным на данную заявку") : ForbiddenException(message);