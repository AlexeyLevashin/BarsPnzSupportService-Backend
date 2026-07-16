using Application.Common.Pagination;
using Application.Common.Validators.Interfaces;
using Application.Dto.Employees.Requests;
using Application.Dto.Employees.Responses;
using Application.Exceptions.Abstractions;
using Application.Exceptions.Employees;
using Application.Exceptions.Users;
using Application.Extensions;
using Application.Interfaces.Repositories;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Mapster;

namespace Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkplaceValidationService _workplaceValidationService;

    public EmployeeService(IEmployeeRepository employeeRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IWorkplaceValidationService workplaceValidationService)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _workplaceValidationService = workplaceValidationService;
    }

    public async Task<Guid> AddAsync(CreateEmployeeRequest request, UserRole userRole, List<Guid> institutionIds)
    {
        if (userRole == UserRole.UserAdmin && !request.Workplaces.Any())
        {
            throw new UserNotBoundToInstitutionException();
        }

        if (userRole == UserRole.UserAdmin && request.Workplaces.Any(reqInst => !institutionIds.Contains(reqInst.InstitutionId)))
        {
            throw new ForeignInstitutionException();
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber)&& await _employeeRepository.CheckByPhoneNumberExistsAsync(request.PhoneNumber))
        {
            throw new EmployeeWithPhoneNumberIsAlreadyExistException();
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && await _employeeRepository.CheckByEmailExistsAsync(request.Email))
        {
            throw new EmployeeWithEmailIsAlreadyExistException();
        }

        await _workplaceValidationService.ValidateAsync(request.Workplaces);
        
        var dbEmployee = request.Adapt<DbEmployee>();

        await _employeeRepository.AddAsync(dbEmployee);
        await _unitOfWork.SaveChangesAsync();

        return dbEmployee.Id;
    }
    
    public async Task<Guid> UpdateAsync(Guid id, CreateEmployeeRequest request, UserRole userRole, List<Guid> institutionIds)
    {
        var employeeToUpdate = await _employeeRepository.GetByIdAsync(id);
        if (employeeToUpdate is null)
        {
            throw new EmployeeNotFoundException();
        }
        
        if (userRole == UserRole.UserAdmin && !request.Workplaces.Any())
        {
            throw new UserNotBoundToInstitutionException();
        }

        if (userRole == UserRole.UserAdmin && !employeeToUpdate.EmployeeInstitutions.Any(ei => institutionIds.Contains(ei.InstitutionId)))
        {
            throw new ForbiddenException("Администратор учреждения может изменять только сотрудников своего учреждения");
        }

        if (userRole == UserRole.UserAdmin && request.Workplaces.Any(reqInst => !institutionIds.Contains(reqInst.InstitutionId)))
        {
            throw new ForeignInstitutionException("Вы не можете привязывать сотрудников к учреждениям, к которым у вас нет доступа");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != employeeToUpdate.PhoneNumber && await _employeeRepository.CheckByPhoneNumberExistsAsync(request.PhoneNumber, id))
        {
            throw new EmployeeWithPhoneNumberIsAlreadyExistException();
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != employeeToUpdate.Email && await _employeeRepository.CheckByEmailExistsAsync(request.Email, id))
        {
            throw new EmployeeWithEmailIsAlreadyExistException();
        }

        await _workplaceValidationService.ValidateAsync(request.Workplaces);
        
        request.Adapt(employeeToUpdate);

        employeeToUpdate.EmployeeInstitutions.SyncEmployeeInstitutions(request.Workplaces);

        await _unitOfWork.SaveChangesAsync();
        return employeeToUpdate.Id;
    }

    public async Task<PagedResponse<GetEmployeeResponse>> GetAllEmployeesAsync(int pageNumber, int pageSize, UserRole userRole, List<Guid> institutionIds)
    { 
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 50) pageSize = 50;
    
        List<Guid>? filterInstitutionId = null;

        if (userRole == UserRole.UserAdmin)
        {
            if (institutionIds == null || !institutionIds.Any())
            {
                throw new UserNotBoundToInstitutionException();
            }
        
            filterInstitutionId = institutionIds;
        }
    
        var (employees, totalCount) = await _employeeRepository.GetAllAsync(pageNumber, pageSize, filterInstitutionId);
    
        var employeeResponses = employees.Adapt<List<GetEmployeeResponse>>();
    
        return new PagedResponse<GetEmployeeResponse>
        {
            Items = employeeResponses,
            PageInfo = new PageViewModel(pageNumber, totalCount, pageSize)
        };
    }
    
    public async Task DeleteAsync(Guid currentUserId, Guid employeeId, UserRole currentUserRole, List<Guid> institutionIds)
    {
        var employeeToDelete = await _employeeRepository.GetByIdAsync(employeeId);
        if (employeeToDelete is null || employeeToDelete.IsDeleted)
        {
            throw new EmployeeNotFoundException();
        }

        if (currentUserRole == UserRole.UserAdmin && !employeeToDelete.EmployeeInstitutions.Any(ei => institutionIds.Contains(ei.InstitutionId)))
        {
            throw new ForbiddenException("Вы не можете удалить сотрудника не из своего учреждения");
        }

        if (employeeToDelete.IsUser)
        {
            var userToDelete = await _userRepository.GetByEmployeeIdAsync(employeeId);

            if (userToDelete is null)
            {
                throw new UserNotFoundException();
            }

            if (userToDelete.Id == currentUserId)
            {
                throw new BadRequestException("Нельзя удалить самого себя");
            }

            if (currentUserRole == UserRole.Operator && (userToDelete.Role == UserRole.SuperAdmin || userToDelete.Role == UserRole.Operator))
            {
                throw new ForbiddenException("У вас нет прав на удаление пользователя с такой ролью");
            }

            if (currentUserRole == UserRole.UserAdmin && userToDelete.Role != UserRole.User)
            {
                throw new ForbiddenException("У вас нет прав на удаление этого пользователя");
            }

            userToDelete.IsDeleted = true;
            employeeToDelete.IsUser = false;
        }
        
        employeeToDelete.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();
    }
}