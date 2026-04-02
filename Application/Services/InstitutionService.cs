using Application.Common.Pagination;
using Application.Dto.Institutions.Requests;
using Application.Dto.Institutions.Responses;
using Application.Exceptions.Institutions;
using Application.Exceptions.Users;
using Application.Interfaces;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Mapster;
using StackExchange.Redis;

namespace Application.Services;

public class InstitutionService : IInstitutionService
{
    private readonly IInstitutionRepository _institutionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public InstitutionService(IInstitutionRepository institutionRepository, IUnitOfWork unitOfWork, IUserRepository userRepository)
    {
        _institutionRepository = institutionRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<GetInstitutionResponse> GetMy(Guid? institutionsId)
    {
        if (institutionsId is null)
        {
            throw new UserNotBoundToInstitutionException();
        }
        
        var institution = await _institutionRepository.GetByIdAsync(institutionsId.Value);

        if (institution is null)
        {
            throw new InstitutionNotFoundException();
        }
        
        return institution.Adapt<GetInstitutionResponse>();
    }

    public async Task<GetInstitutionResponse> AddAsync(CreateInstitutionRequest request)
    {
        var institution = await _institutionRepository.GetByInnAsync(request.INN);
        if (institution is not null)
        {
            throw new InstitutionWithInnIsAlreadyExistException();
        }

        var newInstitution = request.Adapt<DbInstitution>();
        await _institutionRepository.AddAsync(newInstitution);
        await _unitOfWork.SaveChangesAsync();

        return newInstitution.Adapt<GetInstitutionResponse>();
    }

    public async Task<GetInstitutionResponse> GetByIdAsync(Guid? institutionsId)
    {
        if (institutionsId is null)
        {
            throw new UserNotBoundToInstitutionException();
        }
        
        var institution = await _institutionRepository.GetByIdAsync(institutionsId.Value);
        
        if (institution is null)
        {
            throw new InstitutionNotFoundException();
        }
        
        return institution.Adapt<GetInstitutionResponse>();
    }
    
    public async Task<PagedResponse<GetInstitutionResponse>> GetAllAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;

        if (pageSize < 1) pageSize = 20;

        if (pageSize > 50) pageSize = 50;

        var institutionsInfo = await _institutionRepository.GetAllAsync(pageNumber, pageSize);
        var institutions = institutionsInfo.Institutions.Adapt<List<GetInstitutionResponse>>();

        return new PagedResponse<GetInstitutionResponse>
        {
            Items = institutions,
            PageInfo = new PageViewModel(pageNumber, institutionsInfo.totalCount, pageSize)
        };
    }

    public async Task<GetInstitutionResponse> UpdateAsync(CreateInstitutionRequest updateInstitution, Guid id)
    {
        var institution = await _institutionRepository.GetByIdAsync(id);
        if (institution is null)
        {
            throw new InstitutionNotFoundException();
        }
        
        if (!string.Equals(updateInstitution.INN, institution.INN, StringComparison.Ordinal))
        {
            var existingInstitutionByInn = await _institutionRepository.GetByInnAsync(updateInstitution.INN);
            if (existingInstitutionByInn is not null)
            {
                throw new InstitutionWithInnIsAlreadyExistException();
            }
        }
        
        updateInstitution.Adapt(institution);
        _institutionRepository.UpdateAsync(institution);
        await _unitOfWork.SaveChangesAsync();

        return institution.Adapt<GetInstitutionResponse>();
    }

    public async Task DeleteAsync(Guid id)
    {
        var institution = await _institutionRepository.GetByIdAsync(id);
        if (institution is null)
        {
            throw new InstitutionNotFoundException();
        }

        var hasUsers = await _userRepository.CheckUserExistByInstitutionId(id);
        if (hasUsers)
        {
            throw new InstitutionHasEmployeesException();
        }

        _institutionRepository.DeleteAsync(institution);
        await _unitOfWork.SaveChangesAsync();
    }
}