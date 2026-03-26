using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.ViewModels;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class RewardsViewModelTests
{
    [Fact]
    public async Task LoadAsync_LoadsCurrentUsersReward()
    {
        var reward = new TriviaReward
        {
            Id = 5,
            UserId = 10,
            CreatedAt = DateTime.UtcNow,
            IsRedeemed = false,
        };
        var repository = new StubTriviaRewardRepository(reward);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.LoadAsync();

        Assert.Same(reward, viewModel.TriviaReward);
        Assert.True(viewModel.HasTriviaReward);
        Assert.Equal("Free movie ticket — ready to use!", viewModel.TriviaRewardStatusText);
    }

    [Fact]
    public async Task RedeemTriviaRewardAsync_MarksRewardAsRedeemedAndPersistsIt()
    {
        var reward = new TriviaReward
        {
            Id = 5,
            UserId = 10,
            CreatedAt = DateTime.UtcNow,
            IsRedeemed = false,
        };
        var repository = new StubTriviaRewardRepository(reward);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);
        await viewModel.LoadAsync();

        await viewModel.RedeemTriviaRewardAsync();

        Assert.True(reward.IsRedeemed);
        Assert.Equal([5], repository.MarkAsRedeemedCalls);
        Assert.Equal("Already redeemed", viewModel.TriviaRewardStatusText);
    }

    [Fact]
    public async Task RedeemTriviaRewardAsync_DoesNothingWhenThereIsNoReward()
    {
        var repository = new StubTriviaRewardRepository(null);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.RedeemTriviaRewardAsync();

        Assert.Empty(repository.MarkAsRedeemedCalls);
    }

    private sealed class StubTriviaRewardRepository(TriviaReward? reward) : ITriviaRewardRepository
    {
        public List<int> MarkAsRedeemedCalls { get; } = [];

        public Task AddAsync(TriviaReward newReward, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<TriviaReward?> GetUnredeemedByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(reward);
        }

        public Task MarkAsRedeemedAsync(int rewardId, CancellationToken cancellationToken = default)
        {
            MarkAsRedeemedCalls.Add(rewardId);
            return Task.CompletedTask;
        }
    }
}
