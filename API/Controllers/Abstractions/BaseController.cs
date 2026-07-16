using Domain.DbModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Domain.Enums;
using Infrastructure.Extensions;

namespace API.Controllers.Abstractions;

[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected Guid UserId => User.GetUserId();

    protected UserRole UserRole => User.GetUserRole();
    
    protected List<Guid> InstitutionIds => User.GetInstitutionIds();

    protected Guid EmployeeId => User.GetEmployeeId();
}