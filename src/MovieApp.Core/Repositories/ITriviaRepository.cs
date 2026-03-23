using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories
{
    public interface ITriviaRepository
    {
        Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    }
}
