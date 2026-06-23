namespace Application.Interfaces;

public interface IAutoCloseStaleRequestsJob
{
    public Task ExecuteAsync();
}