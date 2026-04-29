using Application.Dto.Messages.Responses;
using Mapster;
using Domain.DbModels;
using Application.Dto.Requests.Responses;

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
            
            .Map(dest => dest.OperatorFullName, 
                src => src.Operator != null 
                    ? $"{src.Operator.Surname} {src.Operator.Name} {src.Operator.Patronymic}".Trim() : null);

        config.NewConfig<DbMessage, GetMessageResponse>()
            .Map(dest => dest.SenderFullName,
                src => $"{src.Sender.Surname} {src.Sender.Name} {src.Sender.Patronymic}".Trim());
    }
}