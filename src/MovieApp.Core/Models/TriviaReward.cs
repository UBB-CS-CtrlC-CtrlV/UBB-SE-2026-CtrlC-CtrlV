namespace MovieApp.Core.Models;

public sealed class TriviaReward
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public bool IsRedeemed { get; set; }

    public DateTime CreatedAt { get; init; }

    public void Redeem()
    {
        IsRedeemed = true;
    }

    public bool IsAvailable => !IsRedeemed;
}