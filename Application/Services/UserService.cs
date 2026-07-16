using Application.Common.Pagination;
using Application.Common.Validators.Interfaces;
using Application.Dto.Employees.Requests;
using Application.Dto.Institutions.Requests;
using Application.Dto.Users.Requests;
using Application.Dto.Users.Responses;
using Application.Dto.UserWithEmployee.Requests;
using Application.Exceptions.Abstractions;
using Application.Exceptions.Employees;
using Application.Exceptions.Institutions;
using Application.Exceptions.JobTitles;
using Application.Exceptions.Users;
using Application.Extensions;
using Application.Interfaces;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Mapster;
using StackExchange.Redis;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IInstitutionRepository _institutionRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IJobTitleRepository _jobTitleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IWorkplaceValidationService _workplaceValidationService;

    public UserService(IUserRepository userRepository, IInstitutionRepository institutionRepository, IUnitOfWork unitOfWork, IPasswordService passwordService, IEmployeeRepository employeeRepository, IJobTitleRepository jobTitleRepository, IWorkplaceValidationService workplaceValidationService)
    {
        _userRepository = userRepository;
        _institutionRepository = institutionRepository;
        _employeeRepository = employeeRepository;
        _jobTitleRepository = jobTitleRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _workplaceValidationService = workplaceValidationService;
    }

    public async Task<GetUserResponse> GetMeAsync(Guid? userId)
    {
        if (Guid.Empty == userId)
        {
            throw new BadRequestException("ID пользователя не может быть пустым");
        }
        
        var user = await _userRepository.GetByIdWithDeyailsAsync(userId);

        if (user is null)
        {
            throw new UserNotFoundException();
        }
        
        return user.Adapt<GetUserResponse>();
    }
    
    public async Task<CreateUserResponse> AddAsync(CreateUserByAdminRequest request, Guid employeeId, UserRole userRole, List<Guid> institutionIds)
    {
        if (userRole == UserRole.UserAdmin)
        {
            request.Role = UserRole.User;
        }
        
        if (userRole == UserRole.Operator && request.Role == UserRole.SuperAdmin)
        {
            throw new ForbiddenException("Оператор не может добавлять суперадмина");
        }

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee is null)
        {
            throw new EmployeeNotFoundException();
        }
        
        if (employee.IsUser)
        {
            throw new ConflictException("У данного сотрудника уже есть учетная запись");
        }

        if ((request.Role == UserRole.User || request.Role == UserRole.UserAdmin)
            && !employee.EmployeeInstitutions.Any())
        {
            throw new UserNotBoundToInstitutionException(
                "Сначала добавьте сотрудника в одно из учреждений");
        }
        
        if (userRole == UserRole.UserAdmin && !employee.EmployeeInstitutions.Any(ei => institutionIds.Contains(ei.InstitutionId)))
        {
            throw new ForeignInstitutionException();
        }
        
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is not null)
        {
            throw new UserWithEmailIsAlreadyExistException();
        }
        
        if (await _employeeRepository.CheckByEmailExistsAsync(request.Email, employeeId))
        {
            throw new EmployeeWithEmailIsAlreadyExistException();
        }
        
        var newUser = request.Adapt<DbUser>();
        newUser.EmployeeId = employeeId;
        var password = _passwordService.GeneratePassword();
        newUser.PasswordHash = _passwordService.Hash(password);
        employee.IsUser = true;
        employee.Email = request.Email;
        await _userRepository.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();
        
        var response = newUser.Adapt<CreateUserResponse>();
        response.InitialPassword = password;
        return response;
    }

    public async Task<CreateUserResponse> AddEmployeeWithUserAsync(CreateUserWithEmployeeRequest request, UserRole userRole, List<Guid> institutionIds)
    {
        if (userRole == UserRole.UserAdmin)
        {
            request.Role = UserRole.User;
        }
        
        if (userRole == UserRole.Operator && request.Role == UserRole.SuperAdmin)
        {
            throw new ForbiddenException("Оператор не может добавлять суперадмина");
        }

        if ((request.Role == UserRole.User || request.Role == UserRole.UserAdmin) && !request.Workplaces.Any())
        {
            throw new UserNotBoundToInstitutionException(
                "Для роли «Пользователь» или «Админ учреждения» необходимо указать хотя бы одно учреждение");
        }

        if (userRole == UserRole.UserAdmin && request.Workplaces.Any(reqInst => !institutionIds.Contains(reqInst.InstitutionId)))
        {
            throw new ForeignInstitutionException();
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber)&& await _employeeRepository.CheckByPhoneNumberExistsAsync(request.PhoneNumber))
        {
            throw new EmployeeWithPhoneNumberIsAlreadyExistException();
        }
        
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user is not null)
        {
            throw new UserWithEmailIsAlreadyExistException();
        }
    
        if (await _employeeRepository.CheckByEmailExistsAsync(request.Email))
        {
            throw new EmployeeWithEmailIsAlreadyExistException();
        }
        
        await _workplaceValidationService.ValidateAsync(request.Workplaces);
        
        var newUser = request.Adapt<DbUser>();
        var password = _passwordService.GeneratePassword();
        newUser.PasswordHash = _passwordService.Hash(password);
        
        await _userRepository.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();
        
        var response = newUser.Adapt<CreateUserResponse>();
        response.InitialPassword = password;
        return response;
    }

    public async Task<GetUserResponse> GetUserById(Guid? userId)
    {
        if (Guid.Empty == userId)
        {
            throw new BadRequestException("ID пользователя не может быть пустым");
        }
        
        var user = await _userRepository.GetByIdWithDeyailsAsync(userId);

        if (user is null)
        {
            throw new UserNotFoundException();
        }
        
        return user.Adapt<GetUserResponse>();
    }

    public async Task<GetUserResponse> GetUserByEmployeeIdAsync(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
        {
            throw new BadRequestException("ID сотрудника не может быть пустым");
        }

        var user = await _userRepository.GetByEmployeeIdAsync(employeeId);

        if (user is null)
        {
            throw new UserNotFoundException();
        }

        return user.Adapt<GetUserResponse>();
    }

    public async Task<PagedResponse<GetUserResponse>> GetAllUsersAsync(int pageNumber, int pageSize, UserRole userRole, List<Guid> institutionIds)
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
        
        var usersInfo = await _userRepository.GetAllAsync(pageNumber, pageSize, filterInstitutionId);
        var users = usersInfo.Users.Adapt<List<GetUserResponse>>();
        
        return new PagedResponse<GetUserResponse>
        {
            Items = users,
            PageInfo = new PageViewModel(pageNumber, usersInfo.totalCount, pageSize)
        };
    }

    public async Task<List<GetOperatorResponse>> GetSupervisorsAsync()
    {
        var roles = new List<UserRole> { UserRole.Operator, UserRole.SuperAdmin };
        var users = await _userRepository.GetByRolesAsync(roles);
        var result = users.Adapt<List<GetOperatorResponse>>();
        return result;
    }
    
    public async Task<GetUserResponse> UpdateAsync(CreateUserWithEmployeeRequest request, Guid userId, Guid id, UserRole userRole, List<Guid> institutionIds)
    {
        var userToUpdate = await _userRepository.GetByIdAsync(id);
        if (userToUpdate is null)
        {
            throw new UserNotFoundException();
        }
        
        var employee = userToUpdate.Employee;
        
        if (userId != id && userRole == UserRole.User)
        {
            throw new ForbiddenException("Пользователь не может изменять других пользователей");
        }
        
        if (userId != id && userRole == UserRole.UserAdmin && (userToUpdate.Role != UserRole.User || !userToUpdate.Employee.EmployeeInstitutions.Any(ei => institutionIds.Contains(ei.InstitutionId))))
        {
            throw new ForbiddenException("Администратор учреждения может изменять только обычных пользователей и только в пределах своего учреждения");
        }
        
        if (userId != id && userRole == UserRole.Operator && (userToUpdate.Role == UserRole.SuperAdmin || userToUpdate.Role == UserRole.Operator))
        {
            throw new ForbiddenException("У вас нет прав на изменение данного пользователя");
        }
        
        if (userRole == UserRole.Operator && (request.Role == UserRole.SuperAdmin || request.Role == UserRole.Operator))
        {
            throw new ForbiddenException("Оператор не может выдавать такие права");
        }

        if (userRole == UserRole.UserAdmin && request.Workplaces.Any(w => !institutionIds.Contains(w.InstitutionId)))
        {
            throw new ForeignInstitutionException("Вы не можете привязывать сотрудников к учреждениям, к которым у вас нет доступа");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber)&& request.PhoneNumber != employee.PhoneNumber && await _employeeRepository.CheckByPhoneNumberExistsAsync(request.PhoneNumber, employee.Id))
        {
            throw new EmployeeWithPhoneNumberIsAlreadyExistException();
        }
        
        if (await _employeeRepository.CheckByEmailExistsAsync(request.Email, employee.Id))
        {
            throw new EmployeeWithEmailIsAlreadyExistException();
        }
        
        if (userRole == UserRole.User)
        {
            request.Workplaces = employee.EmployeeInstitutions.Select(ei => new EmployeeInstitutionRequest 
            { 
                InstitutionId = ei.InstitutionId, 
                JobTitleId = ei.JobTitleId 
            }).ToList();
        }
        
        var checkEmailExist = await _userRepository.GetByEmailAsync(request.Email);
        if (checkEmailExist is not null && checkEmailExist.Id != id)
        {
            throw new UserWithEmailIsAlreadyExistException();
        }
        
        if (userRole == UserRole.UserAdmin || userRole == UserRole.User)
        {
            request.Role = (userId == id) ? userToUpdate.Role : UserRole.User;
        }

        if ((request.Role == UserRole.User || request.Role == UserRole.UserAdmin) && !request.Workplaces.Any())
        {
            throw new UserNotBoundToInstitutionException(
                "Нельзя удалить единственное учреждение у пользователя с ролью «Пользователь» или «Админ учреждения»");
        }

        await _workplaceValidationService.ValidateAsync(request.Workplaces);
        
        userToUpdate.Email = request.Email;
        userToUpdate.Role = request.Role;
    
        employee.Name = request.Name;
        employee.Surname = request.Surname;
        employee.Patronymic = request.Patronymic;
        employee.PhoneNumber = request.PhoneNumber;
        employee.Email = request.Email;
        
        employee.EmployeeInstitutions.SyncEmployeeInstitutions(request.Workplaces);
        
        await _unitOfWork.SaveChangesAsync();

        return userToUpdate.Adapt<GetUserResponse>();
    }

    public async Task UpdatePasswordAsync(UpdateUserPasswordRequest request, Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new UserNotFoundException();
        }

        if (!_passwordService.Verify(request.Password, user.PasswordHash))
        {
            throw new FailureAuthorizationException();
        }

        user.PasswordHash = _passwordService.Hash(request.NewPassword);
        _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CreateUserResponse> ForceResetPasswordAsync(Guid userId, Guid id, UserRole userRole, List<Guid> institutionIds)
    {
        // todo Когда будет реализована работа с почтой, раскомментить проверку
        // if (userId == id)
        // {
        //     throw new BadRequestException("Нельзя поменять пароль самому себе без подтверждения почты");
        // }
        var userToUpdate = await _userRepository.GetByIdAsync(id);
        if (userToUpdate is null)
        {
            throw new UserNotFoundException();
        }

        if (userId != id && userRole == UserRole.UserAdmin && (userToUpdate.Role != UserRole.User || !userToUpdate.Employee.EmployeeInstitutions.Any(w => institutionIds.Contains(w.InstitutionId))))
        {
            throw new ForbiddenException("У вас нет прав на изменение пароля для данного пользователя");
        }
        
        if (userId != id && userRole == UserRole.Operator && (userToUpdate.Role == UserRole.SuperAdmin || userToUpdate.Role == UserRole.Operator))
        {
            throw new ForbiddenException("У вас нет прав на изменение пароля для данного пользователя");
        }
        
        var newPassword = _passwordService.GeneratePassword();
        userToUpdate.PasswordHash = _passwordService.Hash(newPassword);
        _userRepository.UpdateAsync(userToUpdate);
        await _unitOfWork.SaveChangesAsync();

        var response = userToUpdate.Adapt<CreateUserResponse>();
        response.InitialPassword = newPassword;
        return response;
    }

    public async Task RevoteAccessAsync(Guid userId, Guid id, UserRole userRole, List<Guid> institutionIds)
    {
        if (userId == id)
        {
            throw new BadRequestException("Нельзя удалить самого себя");
        }
        
        var userToDelete = await _userRepository.GetByIdAsync(id);
        
        if (userToDelete is null)
        {
            throw new UserNotFoundException();
        }
        
        if (userRole == UserRole.UserAdmin && userToDelete.Role != UserRole.User)
        {
            throw new ForbiddenException("У вас нет прав на удаление этого пользователя");
        }

        if (userRole == UserRole.UserAdmin && !userToDelete.Employee.EmployeeInstitutions.Any(w => institutionIds.Contains(w.InstitutionId)))
        {
            throw new ForbiddenException("Нельзя удалить пользователя не из своего учреждения");
        }

        if (userRole == UserRole.Operator && (userToDelete.Role == UserRole.SuperAdmin || userToDelete.Role == UserRole.Operator))
        {
            throw new ForbiddenException("У вас нет прав на удаление данного пользователя");
        }

        var employee = await _employeeRepository.GetByIdAsync(userToDelete.EmployeeId);
        if (employee is null)
        {
            throw new EmployeeNotFoundException();
        }

        userToDelete.IsDeleted = true;
        employee.IsUser = false;
        await _unitOfWork.SaveChangesAsync();
    }
}