using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Auth;

namespace OnlineCinema.Backend.Mappings;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<RegisterRequestDto, User>();
        CreateMap<User, UserDto>();
        CreateMap<User, AuthResponseDto>();
    }
}