using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.UserActions;

namespace OnlineCinema.Backend.Mappings;

public class UserActionProfile : Profile
{
    public UserActionProfile()
    {
        CreateMap<UserMovieStatus, UserMovieDto>()
            .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
            .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie.Title))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AddedAt, opt => opt.MapFrom(src => src.AddedAt))
            .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.Movie.PosterUrl));
    }
}
