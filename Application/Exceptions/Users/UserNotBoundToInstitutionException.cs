using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Users;

public class UserNotBoundToInstitutionException(string? message ="Учетная запись должна быть связана с учреждением") : BadRequestException(message);