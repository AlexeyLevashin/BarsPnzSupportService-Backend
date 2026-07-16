using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Requests;

public class NoInstitutionSelected(string? message = "Необходимо выбрать учреждение для создания заявки") : BadRequestException(message);