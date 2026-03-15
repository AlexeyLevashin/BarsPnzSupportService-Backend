namespace Application.Exceptions.Abstractions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string? message) : base(message) { }
}