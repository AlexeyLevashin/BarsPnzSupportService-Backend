using API.Controllers.Abstractions;
using Application.Dto.Users.Requests;
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
    
    [HttpPost]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> Add(CreateUserByAdminRequest request)
    {
        return Ok(await _userService.AddAsync(request, UserId, UserRole, InstitutionId));
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
        return Ok(await _userService.GetAllUsers(page, pageSize, UserRole, InstitutionId));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreateUserByAdminRequest request)
    {
        return Ok(await _userService.UpdateAsync(request, UserId, id, UserRole, InstitutionId));
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
        return Ok(await _userService.ForceResetPasswordAsync(UserId, id, UserRole, InstitutionId));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteAsync(UserId, id, UserRole, InstitutionId);
        return NoContent();
    }
}