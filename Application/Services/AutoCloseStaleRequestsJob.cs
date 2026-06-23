using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class AutoCloseStaleRequestsJob : IAutoCloseStaleRequestsJob
{
    private readonly IRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AutoCloseStaleRequestsJob(IRequestRepository requestRepository, IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task ExecuteAsync()
    {
        var deadline = DateTime.UtcNow.AddDays(-3);

        var staleRequests = await _requestRepository.GetStaleRequestsAsync(deadline);
        
        if(!staleRequests.Any()) return;

        foreach (var r in staleRequests)
        {
            r.Status = RequestStatus.Closed;
            r.ClosedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
    }
}