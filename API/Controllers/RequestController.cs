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
    public async Task<IActionResult> GetAll(int pageNumber, int pageSize)
    {
        return Ok(await _requestService.GetAllAsync(pageNumber, pageSize));
    }

    [HttpGet("all/my")]
    public async Task<IActionResult> GetMy(int pageNumber, int pageSize)
    {
        return Ok(await _requestService.GetMyAsync(pageNumber, pageSize, UserId));
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "SuperAdmin, Operator")]
    public async Task<IActionResult> AssignToOperator(Guid id)
    {
        return Ok(await _requestService.AssignToOperatorAsync(id, UserId));
    }
}