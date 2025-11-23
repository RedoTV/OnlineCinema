using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Actors;

namespace OnlineCinema.Backend.Mappings;

public class ActorProfile : Profile
{
    public ActorProfile()
    {
        CreateMap<Actor, ActorDto>();
        CreateMap<CreateActorDto, Actor>();
        CreateMap<UpdateActorDto, Actor>();
    }
}