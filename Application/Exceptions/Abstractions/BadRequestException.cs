namespace Application.Exceptions.Abstractions;

public class BadRequestException : Exception
{
    public BadRequestException(string? message) : base(message) { }
}