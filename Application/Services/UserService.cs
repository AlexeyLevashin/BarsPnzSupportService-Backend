using Application.Common.Pagination;
using Application.Dto.Institutions.Requests;
using Application.Dto.Users.Requests;
using Application.Dto.Users.Responses;
using Application.Exceptions.Abstractions;
using Application.Exceptions.Institutions;
using Application.Exceptions.Users;
using Application.Interfaces;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Mapster;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IInstitutionRepository _institutionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;

    public UserService(IUserRepository userRepository, IInstitutionRepository institutionRepository, IUnitOfWork unitOfWork, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _institutionRepository = institutionRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    public async Task<GetUserResponse> GetMeAsync(Guid? userId)
    {
        if (Guid.Empty == userId)
        {
            throw new BadRequestException("ID пользователя не может быть пустым");
        }
        
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new UserNotFoundException();
        }
        
        return user.Adapt<GetUserResponse>();
    }
    
    public async Task<CreateUserResponse> AddAsync(CreateUserByAdminRequest request, Guid userId, UserRole userRole, Guid? institutionId)
    {
        if (userRole == UserRole.UserAdmin)
        {
            request.Role = UserRole.User;
            request.InstitutionId = institutionId;
        }
        
        if (userRole == UserRole.Operator && request.Role == UserRole.SuperAdmin)
        {
            throw new ForbiddenException("Оператор не может добавлять суперадмина");
        }

        if ((request.Role == UserRole.User || request.Role == UserRole.UserAdmin) && request.InstitutionId is null)
        {
            throw new UserNotBoundToInstitutionException();
        }
        
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user is not null)
        {
            throw new UserWithEmailIsAlreadyExistException();
        }

        if (request.InstitutionId != null)
        {
            var institution = await _institutionRepository.GetByIdAsync(request.InstitutionId.Value);

            if (institution is null)
            {
                throw new InstitutionNotFoundException();
            }
        }
    
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
        
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new UserNotFoundException();
        }
        
        return user.Adapt<GetUserResponse>();
    }

    public async Task<PagedResponse<GetUserResponse>> GetAllUsers(int pageNumber, int pageSize, UserRole userRole, Guid? institutionId)
    { 
        if (pageNumber < 1) pageNumber = 1;

        if (pageSize < 1) pageSize = 20;

        if (pageSize > 50) pageSize = 50;
        
        Guid? filterInstitutionId = null;

        if (userRole == UserRole.UserAdmin)
        {
            if (institutionId is null)
            {
                throw new UserNotBoundToInstitutionException();
            }
            filterInstitutionId = institutionId;
        }
        var usersInfo = await _userRepository.GetAllAsync(pageNumber, pageSize, filterInstitutionId);
        var users = usersInfo.Users.Adapt<List<GetUserResponse>>();
        
        return new PagedResponse<GetUserResponse>
        {
            Items = users,
            PageInfo = new PageViewModel(pageNumber, usersInfo.totalCount, pageSize)
        };
    }

    public async Task<GetUserResponse> UpdateAsync(CreateUserByAdminRequest request, Guid userId, Guid id, UserRole userRole, Guid? institutionId)
    {
        var userToUpdate = await _userRepository.GetByIdAsync(id);
        if (userToUpdate is null)
        {
            throw new UserNotFoundException();
        }
        
        if (userId != id && userRole == UserRole.User)
        {
            throw new ForbiddenException("Пользователь не может изменять других пользователей");
        }
        
        if (userId != id && userRole == UserRole.UserAdmin && (userToUpdate.Role != UserRole.User || userToUpdate.InstitutionId != institutionId))
        {
            throw new ForbiddenException("Администратор учреждения может изменять только обычных пользователей и только в пределах своего учреждения");
        }

        if (userRole == UserRole.UserAdmin || userRole == UserRole.User)
        {
            request.Role = (userId == id) ? userToUpdate.Role : UserRole.User;
            request.InstitutionId = institutionId;
        }

        if (userId != id && userRole == UserRole.Operator && (userToUpdate.Role == UserRole.SuperAdmin || userToUpdate.Role == UserRole.Operator))
        {
            throw new ForbiddenException("У вас нет прав на изменение данного пользователя");
        }
        
        if (userRole == UserRole.Operator && (request.Role == UserRole.SuperAdmin || request.Role == UserRole.Operator))
        {
            throw new ForbiddenException("Оператор не может выдавать такие права");
        }

        if ((request.Role == UserRole.User || request.Role == UserRole.UserAdmin) && request.InstitutionId is null)
        {
            throw new UserNotBoundToInstitutionException();
        }

        var checkEmailExist = await _userRepository.GetByEmailAsync(request.Email);
        if (checkEmailExist is not null && checkEmailExist.Id != id)
        {
            throw new UserWithEmailIsAlreadyExistException();
        }
        
        if  (request.InstitutionId != null)
        {
            var institution = await _institutionRepository.GetByIdAsync(request.InstitutionId.Value);

            if (institution is null)
            {
                throw new InstitutionNotFoundException();
            }
        }

        request.Adapt(userToUpdate);
        _userRepository.UpdateAsync(userToUpdate);
        await _unitOfWork.SaveChangesAsync();

        var response = userToUpdate.Adapt<GetUserResponse>();
        return response;
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

    public async Task<CreateUserResponse> ForceResetPasswordAsync(Guid userId, Guid id, UserRole userRole, Guid? institutionId)
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

        if (userId != id && userRole == UserRole.UserAdmin && (userToUpdate.Role != UserRole.User || userToUpdate.InstitutionId != institutionId))
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

    public async Task DeleteAsync(Guid userId, Guid id, UserRole userRole, Guid? institutionId)
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

        if (userRole == UserRole.UserAdmin && institutionId != userToDelete.InstitutionId)
        {
            throw new ForbiddenException("Нельзя удалить пользователя не из своего учреждения");
        }

        if (userRole == UserRole.Operator && (userToDelete.Role == UserRole.SuperAdmin || userToDelete.Role == UserRole.Operator))
        {
            throw new ForbiddenException("У вас нет прав на удаление данного пользователя");
        }

        _userRepository.DeleteAsync(userToDelete);
        await _unitOfWork.SaveChangesAsync();
    }
}