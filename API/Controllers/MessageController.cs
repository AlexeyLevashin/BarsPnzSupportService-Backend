using API.Controllers.Abstractions;
using Application.Dto.Messages.Requests;
using Application.Interfaces;
using CommunityToolkit.HighPerformance.Helpers;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace API.Controllers;

[Route("api/messages")]
public class MessageController : BaseController
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromForm] CreateMessageRequest request)
    {
        return Ok(await _messageService.AddAsync(request, UserId, UserRole));
    }
}