namespace MovieApp.Core.Repositories;

public interface IUserEventAttendanceRepository
{
    Task<IReadOnlyList<int>> GetJoinedEventIdsAsync(int userId, CancellationToken cancellationToken = default);

    Task JoinAsync(int userId, int eventId, CancellationToken cancellationToken = default);
}
