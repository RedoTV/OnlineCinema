using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Movies;

namespace OnlineCinema.Backend.Repositories.Movies;

public interface IMovieRepository
{
    Task<IEnumerable<MovieSummaryDto>> GetSummariesAsync(string? search, int? genreId, CancellationToken ct = default);
    Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Movie?> FindAsync(int id, CancellationToken ct = default);
    void Add(Movie movie);
    void Remove(Movie movie);
    Task<List<Genre>> GetGenresByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<List<Actor>> GetActorsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}