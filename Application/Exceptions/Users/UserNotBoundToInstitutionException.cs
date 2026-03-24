using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Users;

public class UserNotBoundToInstitutionException(string? message ="Пользователь запрашивающий данные, должен быть привязан к учрежению") : BadRequestException(message);