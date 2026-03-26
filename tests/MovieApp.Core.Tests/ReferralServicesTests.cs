using System.Text.RegularExpressions;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class ReferralServicesTests
{
    [Fact]
    public void Generate_UsesStableUppercasePatternForSameInputs()
    {
        var generator = new ReferralCodeGenerator();

        var code = generator.Generate("alice", 7);

        Assert.Equal($"ALICE{DateTime.UtcNow.Year}7", code);
    }

    [Fact]
    public void Generate_ReturnsAlphanumericCode_AccordingToRequirements()
    {
        var generator = new ReferralCodeGenerator();

        var code = generator.Generate("alice example", 7);

        Assert.Matches(new Regex("^[A-Za-z0-9]+$"), code);
    }

    [Fact]
    public async Task IsValidReferralAsync_ReturnsFalseWhenCodeDoesNotExist()
    {
        var repository = new StubAmbassadorRepository();
        var validator = new ReferralValidator(repository);

        var result = await validator.IsValidReferralAsync("MISSING", currentUserId: 10);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidReferralAsync_ReturnsFalseForOwnCode()
    {
        var repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["OWNCODE"] = 10 },
        };
        var validator = new ReferralValidator(repository);

        var result = await validator.IsValidReferralAsync("OWNCODE", currentUserId: 10);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidReferralAsync_ReturnsTrueForAnotherUsersCode()
    {
        var repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
        };
        var validator = new ReferralValidator(repository);

        var result = await validator.IsValidReferralAsync("FRIENDCODE", currentUserId: 10);

        Assert.True(result);
    }

    [Fact]
    public async Task LogReferralUsageAsync_AddsLogAndAttemptsRewardWhenCodeExists()
    {
        var repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
        };
        var service = new ReferralLogService(repository);

        await service.LogReferralUsageAsync("FRIENDCODE", friendId: 10, eventId: 88);

        var loggedUsage = Assert.Single(repository.LogEntries);
        Assert.Equal((42, 10, 88), loggedUsage);
        Assert.Equal([42], repository.TryApplyRewardCalls);
    }

    [Fact]
    public async Task LogReferralUsageAsync_DoesNothingWhenCodeDoesNotExist()
    {
        var repository = new StubAmbassadorRepository();
        var service = new ReferralLogService(repository);

        await service.LogReferralUsageAsync("MISSING", friendId: 10, eventId: 88);

        Assert.Empty(repository.LogEntries);
        Assert.Empty(repository.TryApplyRewardCalls);
    }

    private sealed class StubAmbassadorRepository : IAmbassadorRepository
    {
        public Dictionary<string, int> OwnersByCode { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<(int AmbassadorId, int FriendId, int EventId)> LogEntries { get; } = [];

        public List<int> TryApplyRewardCalls { get; } = [];

        public Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OwnersByCode.ContainsKey(referralCode));
        }

        public Task<string?> GetReferralCodeAsync(int userId, CancellationToken cancellationToken = default)
        {
            var code = OwnersByCode.FirstOrDefault(entry => entry.Value == userId).Key;
            return Task.FromResult(string.IsNullOrWhiteSpace(code) ? null : code);
        }

        public Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken cancellationToken = default)
        {
            OwnersByCode[referralCode] = userId;
            return Task.CompletedTask;
        }

        public Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OwnersByCode.TryGetValue(referralCode, out var ownerId) ? (int?)ownerId : null);
        }

        public Task AddReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default)
        {
            LogEntries.Add((ambassadorId, friendId, eventId));
            return Task.CompletedTask;
        }

        public Task<bool> TryApplyRewardAsync(int ambassadorId, CancellationToken cancellationToken = default)
        {
            TryApplyRewardCalls.Add(ambassadorId);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<ReferralHistoryItem>> GetReferralHistoryAsync(int ambassadorId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<ReferralHistoryItem>>([]);
        }

        public Task<int> GetRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task DecrementRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
