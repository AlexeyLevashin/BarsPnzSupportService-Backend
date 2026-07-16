using API.Controllers.Abstractions;
using Application.Dto.JobTitles.Requests;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/job-titles")]
public class JobTitleController : BaseController
{
    private readonly IJobTitleService _jobTitleService;

    public JobTitleController(IJobTitleService jobTitleService)
    {
        _jobTitleService = jobTitleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _jobTitleService.GetAllAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Operator, UserAdmin, SuperAdmin")]
    public async Task<IActionResult> Add(CreateJobTitleRequest request)
    {
        return Ok(await _jobTitleService.AddAsync(request));
    }
}
