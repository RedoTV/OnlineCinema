using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Movies;

namespace OnlineCinema.Backend.Mappings;

public class MovieProfile : Profile
{
    public MovieProfile()
    {
        CreateMap<Movie, MovieDto>()
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                src.Ratings.Any() ? src.Ratings.Average(r => r.RatingValue) : 0));

        CreateMap<CreateMovieDto, Movie>()
            .ForMember(dest => dest.Genres, opt => opt.Ignore())
            .ForMember(dest => dest.Actors, opt => opt.Ignore());

        CreateMap<UpdateMovieDto, Movie>()
            .ForMember(dest => dest.Genres, opt => opt.Ignore())
            .ForMember(dest => dest.Actors, opt => opt.Ignore());
    }
}