namespace MovieApp.Services;

public sealed class BootstrapUserOptions
{
    public required string AuthProvider { get; init; }

    public required string AuthSubject { get; init; }
}
