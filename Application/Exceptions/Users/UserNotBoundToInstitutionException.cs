using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Users;

public class UserNotBoundToInstitutionException(string? message ="Пользователи с ролями \"Пользователь\" или \"Админ Учреждения\" должены быть привязаны к учрежению") : BadRequestException(message);