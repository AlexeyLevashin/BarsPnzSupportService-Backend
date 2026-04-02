using API.Controllers.Abstractions;
using Application.Dto.Institutions.Requests;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/institutions")]
public class InstitutionController : BaseController
{
    private readonly IInstitutionService _institutionService;

    public InstitutionController(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        return Ok(await _institutionService.GetMy(InstitutionId));
    }
    
    [HttpPost]
    [Authorize(Roles = "SuperAdmin, Operator")]
    public async Task<IActionResult> Add(CreateInstitutionRequest request)
    {
        return Ok(await _institutionService.AddAsync(request));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin, Operator")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _institutionService.GetByIdAsync(id));
    }
    
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Operator")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _institutionService.GetAllAsync(page, pageSize));
    }

    [HttpPut ("{id}")]
    [Authorize(Roles = "SuperAdmin, Operator")]
    public async Task<IActionResult> Update(Guid id, CreateInstitutionRequest request)
    {
        return Ok(await _institutionService.UpdateAsync(request, id));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin, Operator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _institutionService.DeleteAsync(id);
        return NoContent();
    }
}