using AutoMapper;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;

namespace VRefSoltutions.Profiles
{

    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventResponseDTO>()
                // Map the DTO's message depending on the Event type. Overwrite values if overwrite values are defined
                .ForMember(dto => dto.Message, opt => opt.MapFrom(srcEvent => string.IsNullOrEmpty(srcEvent.OverwriteMessage) ? srcEvent.EventType.Message : srcEvent.OverwriteMessage))
                .ForMember(dto => dto.Name, opt => opt.MapFrom(srcEvent => string.IsNullOrEmpty(srcEvent.OverwriteName) ? srcEvent.EventType.Name : srcEvent.OverwriteName))
                .ForMember(dto => dto.Symbol, opt => opt.MapFrom(srcEvent => string.IsNullOrEmpty(srcEvent.OverwriteSymbol) ? srcEvent.EventType.Symbol: srcEvent.OverwriteSymbol));
        }
    }
}