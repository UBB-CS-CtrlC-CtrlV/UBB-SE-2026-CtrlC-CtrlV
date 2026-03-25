using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

public interface IUserSlotMachineStateRepository
{
    Task<UserSpinData?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task CreateAsync(UserSpinData state, CancellationToken cancellationToken = default);

    Task UpdateAsync(UserSpinData state, CancellationToken cancellationToken = default);
}
