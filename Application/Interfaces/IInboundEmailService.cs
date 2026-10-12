using Application.Dto.EmailMessages.Requests;

namespace Application.Interfaces;

public interface IInboundEmailService
{
    Task ProcessAsync(InboundEmailRequest email, CancellationToken cancellationToken = default);
}
