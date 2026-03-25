using MovieApp.Core.Models.Movie;

namespace MovieApp.Core.Repositories;

public interface IMovieRepository
{
    Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Movie>> FindMoviesByCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> FindScreeningEventIdsForMovieAsync(int movieId, CancellationToken cancellationToken = default);
}
