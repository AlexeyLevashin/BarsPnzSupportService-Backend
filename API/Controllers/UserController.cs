using API.Controllers.Abstractions;
using Application.Dto.Users.Requests;
using Application.Dto.UserWithEmployee.Requests;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/users")]
public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        return Ok(await _userService.GetMeAsync(UserId));
    }

    [HttpGet("by-employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployeeId(Guid employeeId)
    {
        return Ok(await _userService.GetUserByEmployeeIdAsync(employeeId));
    }
    
    [HttpPost("{employeeId}")]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> Add(Guid employeeId, CreateUserByAdminRequest request)
    {
        return Ok(await _userService.AddAsync(request, employeeId, UserRole, InstitutionIds));
    }

    [HttpPost("with-employee")]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> AddEmployeeWithUser(CreateUserWithEmployeeRequest request)
    {
        return Ok(await _userService.AddEmployeeWithUserAsync(request, UserRole, InstitutionIds));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _userService.GetUserById(id));
    }    
    
    [HttpGet]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _userService.GetAllUsersAsync(page, pageSize, UserRole, InstitutionIds));
    }
    
    [HttpGet("operators")]
    [Authorize(Roles = "Operator, SuperAdmin")]
    public async Task<IActionResult> GetSupervisors()
    {
        return Ok(await _userService.GetSupervisorsAsync());
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreateUserWithEmployeeRequest request)
    {
        return Ok(await _userService.UpdateAsync(request, UserId, id, UserRole, InstitutionIds));
    }

    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword(UpdateUserPasswordRequest request)
    {
        await _userService.UpdatePasswordAsync(request, UserId);
        return NoContent();
    }

    [HttpPut("{id}/reset-password")]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> ForceResetPassword(Guid id)
    {
        return Ok(await _userService.ForceResetPasswordAsync(UserId, id, UserRole, InstitutionIds));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> RevoteAccess(Guid id)
    {
        await _userService.RevoteAccessAsync(UserId, id, UserRole, InstitutionIds);
        return NoContent();
    }
}