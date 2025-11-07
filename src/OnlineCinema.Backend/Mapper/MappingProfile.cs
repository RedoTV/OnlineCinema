using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs;

namespace OnlineCinema.Backend.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterRequestDto, User>();
        CreateMap<User, UserDto>();
        CreateMap<User, AuthResponseDto>();
    }
}