using Application.Exceptions.Abstractions;

namespace Application.Exceptions.Employees;

public class ForeignInstitutionException(string? message ="Нельзя совершать действия с сотрудниками учреждений, к которым вы не относитесь") : ForbiddenException(message);