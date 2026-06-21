using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class UnacceptableRequestException(string? message = "Нельзя совершать действия с чужими заявками") : ForbiddenException(message);