namespace LogiFlow.WebApi.Auth;

public sealed class DemoAuthOptions
{
    public List<DemoUserOptions> Users { get; init; } = [];
}

public sealed class DemoUserOptions
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}