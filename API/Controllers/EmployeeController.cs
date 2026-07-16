using API.Controllers.Abstractions;
using Application.Dto.Employees.Requests;
using Application.Dto.UserWithEmployee.Requests;
using Application.Interfaces.Repositories;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/employees")]
public class EmployeeController : BaseController
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }
    
    [HttpPost]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> Add(CreateEmployeeRequest request)
    {
        return Ok(await _employeeService.AddAsync(request, UserRole, InstitutionIds));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(CreateEmployeeRequest request, Guid id)
    {
        return Ok(await _employeeService.UpdateAsync(id, request, UserRole, InstitutionIds));
    }

    [HttpGet]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> GetAllEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _employeeService.GetAllEmployeesAsync(page, pageSize, UserRole, InstitutionIds));
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _employeeService.DeleteAsync(UserId, id, UserRole, InstitutionIds);
        return NoContent();
    }
}