using API.Controllers.Abstractions;
using Application.Dto.Requests.Requests;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/requests")]
public class RequestController : BaseController
{
    private readonly IRequestService _requestService;

    public RequestController(IRequestService requestService)
    {
        _requestService = requestService;
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

    [HttpPatch("{id}")]
    [Authorize(Roles = "SuperAdmin, Operator")]
    public async Task<IActionResult> AssignToOperator(Guid id)
    {
        await _requestService.AssignToOperatorAsync(id, UserId);
        return Ok();
    }
}