using AutoMapper;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;

namespace VRefSoltutions.Profiles
{

    public class TrainingProfile : Profile
    {
        public TrainingProfile()
        {
            CreateMap<Training, TrainingResponseDTO>()
                // Map the Instructor to the 
                .ForMember(dto => dto.Instructor, opt =>
                            opt.MapFrom(srcTraining => srcTraining.Participants
                            .Where(u => u.UserType == Role.Instructor)
                            .First())
                )
                // Map all student participants to the DTO.Students property
                .ForMember(dto => dto.Students, opt =>
                            opt.MapFrom(srcTraining => srcTraining.Participants
                            .Where(u => u.UserType == Role.Student)
                            .Select(u => u)
                            .ToList()));

            CreateMap<Training, TrainingsResponseDTO>()
            // Map the Instructor to the 
                .ForMember(dto => dto.Instructor, opt =>
                            opt.MapFrom(srcTraining => srcTraining.Participants
                            .Where(u => u.UserType == Role.Instructor)
                            .First())
                )
                // Map all student participants to the DTO.Students property
                .ForMember(dto => dto.Students, opt =>
                            opt.MapFrom(srcTraining => srcTraining.Participants
                            .Where(u => u.UserType == Role.Student)
                            .Select(u => u)
                            .ToList()));
        }
    }
}