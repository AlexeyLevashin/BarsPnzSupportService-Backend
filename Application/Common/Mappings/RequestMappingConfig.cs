using Application.Dto.Messages.Responses;
using Mapster;
using Domain.DbModels;
using Application.Dto.Requests.Responses;
using Application.Dto.Users.Responses;

namespace Application.Common.Mappings;

public class RequestMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DbRequest, GetRequestResponse>()
            .Map(dest => dest.ClientFullName, 
                src => $"{src.Client.Surname} {src.Client.Name} {src.Client.Patronymic}".Trim())
            
            .Map(dest => dest.InstitutionName, 
                src => src.Client.Institution != null ? src.Client.Institution.Name : null)
            
            .Map(dest => dest.Operators, src => src.Operators);

        config.NewConfig<DbMessage, GetMessageResponse>()
            .Map(dest => dest.SenderFullName,
                src => $"{src.Sender.Surname} {src.Sender.Name} {src.Sender.Patronymic}".Trim());
        
        config.NewConfig<DbUser, GetOperatorResponse>()
            .Map(dest => dest.OperatorFullName, src => $"{src.Surname} {src.Name} {src.Patronymic}".Trim());
    }
}