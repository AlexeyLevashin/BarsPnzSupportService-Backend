using Application.Dto.Employees.Requests;
using Application.Dto.Employees.Responses;
using Application.Dto.Institutions.Responses;
using Application.Dto.Messages.Responses;
using Mapster;
using Domain.DbModels;
using Application.Dto.Requests.Responses;
using Application.Dto.Users.Responses;
using Application.Dto.UserWithEmployee.Requests;

namespace Application.Common.Mappings;

public class RequestMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DbRequest, GetRequestResponse>()
            .Map(dest => dest.ClientFullName, 
                src => $"{src.Client.Employee.Surname} {src.Client.Employee.Name} {src.Client.Employee.Patronymic}".Trim())
            .Map(dest => dest.InstitutionName, src => src.Institution != null ? src.Institution.Name : null)
            .Map(dest => dest.Operators, src => src.Operators);

        config.NewConfig<DbMessage, GetMessageResponse>()
            .Map(dest => dest.SenderFullName,
                src => $"{src.Sender.Employee.Surname} {src.Sender.Employee.Name} {src.Sender.Employee.Patronymic}".Trim());
        
        config.NewConfig<DbUser, GetOperatorResponse>()
            .Map(dest => dest.OperatorFullName, src => $"{src.Employee.Surname} {src.Employee.Name} {src.Employee.Patronymic}".Trim());
        
        config.NewConfig<CreateEmployeeRequest, DbEmployee>()
            .Map(dest => dest.EmployeeInstitutions, src => src.Workplaces)
            .Map(dest => dest.IsUser, src => false);
        
        config.NewConfig<CreateUserWithEmployeeRequest, DbEmployee>()
            .Map(dest => dest.EmployeeInstitutions, src => src.Workplaces)
            .Map(dest => dest.IsUser, src => true);
        
        config.NewConfig<CreateUserWithEmployeeRequest, DbUser>()
            .Map(dest => dest.Employee, src => src);
        
        config.NewConfig<DbUser, GetUserResponse>()
            .Map(dest => dest.Name, src => src.Employee.Name)
            .Map(dest => dest.Surname, src => src.Employee.Surname)
            .Map(dest => dest.Patronymic, src => src.Employee.Patronymic)
            .Map(dest => dest.PhoneNumber, src => src.Employee.PhoneNumber)
            .Map(dest => dest.Workplaces, src => src.Employee.EmployeeInstitutions);
        
        config.NewConfig<DbEmployeeInstitution, UserInstitutionResponse>()
            .Map(dest => dest.InstitutionId, src => src.InstitutionId)
            .Map(dest => dest.InstitutionName, src => src.Institution.Name)
            .Map(dest => dest.JobTitleId, src => src.JobTitleId)
            .Map(dest => dest.JobTitleName, src => src.JobTitle != null ? src.JobTitle.Name : null);
        
        config.NewConfig<DbEmployee, GetEmployeeResponse>()
            .Map(dest => dest.Workplaces, src => src.EmployeeInstitutions)
            .Ignore(dest => dest.UserId)
            .AfterMapping((src, dest) =>
            {
                dest.UserId = src.User?.Id;
            });

        config.NewConfig<DbInstitution, GetInstitutionResponse>()
            .Map(dest => dest.HeadName, src => src.Head!.Name)
            .Map(dest => dest.HeadSurname, src => src.Head!.Surname)
            .Map(dest => dest.HeadPatronymic, src => src.Head!.Patronymic);
    }
}