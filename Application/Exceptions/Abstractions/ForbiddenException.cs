namespace Application.Exceptions.Abstractions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string? message) : base(message) { }
}