using API.Controllers.Abstractions;
using Application.Dto.Messages.Requests;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/messages")]
public class MessageController : BaseController
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPatch("{messageId}")]
    public async Task<IActionResult> Update(Guid messageId, UpdateMessageRequest request)
    {
        return Ok(await _messageService.UpdateAsync(messageId, UserId, UserRole, request));
    }
}
