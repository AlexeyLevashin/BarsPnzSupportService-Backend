using API.Controllers.Abstractions;
using Application.Dto.Messages.Requests;
using Application.Dto.Requests.Requests;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/requests")]
public class RequestController : BaseController
{
    private readonly IRequestService _requestService;
    private readonly IMessageService _messageService;

    public RequestController(IRequestService requestService, IMessageService messageService)
    {
        _requestService = requestService;
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateRequestRequest request)
    {
        return Ok(await _requestService.AddAsync(request, UserId));
    }

    [HttpGet("all")]
    [Authorize(Roles = "SuperAdmin, Operator")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _requestService.GetAllAsync(page, pageSize));
    }

    [HttpGet("all/my")]
    public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _requestService.GetMyAsync(page, pageSize, UserId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid? id)
    {
        return Ok(await _requestService.GetRequestByIdAsync(id));
    }
    
    [HttpPatch("{id}")]
    [Authorize(Roles = "SuperAdmin, Operator")]
    public async Task<IActionResult> AssignToOperator(Guid? id)
    {
        await _requestService.AssignToOperatorAsync(id, UserId);
        return Ok();
    }
    
    [HttpPost("{requestId}/messages")]
    public async Task<IActionResult> Add(Guid requestId, [FromForm] CreateMessageRequest request)
    {
        return Ok(await _messageService.AddAsync(requestId, request, UserId, UserRole));
    }

    [HttpGet("{requestId}/messages")]
    public async Task<IActionResult> GetMessages(Guid requestId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _messageService.GetAllMessagesAsync(page, pageSize, requestId));
    }
    
    [HttpGet("{requestId}/comments")]
    [Authorize(Roles = "Operator, SuperAdmin")]
    public async Task<IActionResult> GetComments(Guid requestId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _messageService.GetAllCommentsAsync(page, pageSize, requestId));
    }
}