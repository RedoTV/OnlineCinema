using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Series;

namespace OnlineCinema.Backend.Mappings;

public class SeriesProfile : Profile
{
    public SeriesProfile()
    {
        CreateMap<Series, SeriesDto>()
            .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Ratings.Any() ? s.Ratings.Average(r => r.RatingValue) : 0))
            .ForMember(d => d.SeasonsCount, o => o.MapFrom(s => s.Seasons.Count));

        CreateMap<Season, SeasonDto>();
        CreateMap<Episode, EpisodeDto>();

        CreateMap<CreateSeriesDto, Series>()
            .ForMember(d => d.Genres, o => o.Ignore())
            .ForMember(d => d.Actors, o => o.Ignore());

        CreateMap<UpdateSeriesDto, Series>()
            .ForMember(d => d.Genres, o => o.Ignore())
            .ForMember(d => d.Actors, o => o.Ignore());

        CreateMap<CreateSeasonDto, Season>();
        CreateMap<CreateEpisodeDto, Episode>();
    }
}
