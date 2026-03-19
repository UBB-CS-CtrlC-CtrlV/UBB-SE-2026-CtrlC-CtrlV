namespace MovieApp.Models;

public sealed class User
{
    // This is just a dummy class
    public required int Id { get; init; }

    public required string AuthProvider { get; init; }

    public required string AuthSubject { get; init; }

    public string StableId => $"{AuthProvider}:{AuthSubject}";
}
