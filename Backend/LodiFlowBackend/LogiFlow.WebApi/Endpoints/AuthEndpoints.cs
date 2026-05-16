using System.Security.Claims;
using LogiFlow.WebApi.Auth;
using Microsoft.Extensions.Options;

namespace LogiFlow.WebApi.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/login", (
            LoginRequest request,
            IOptions<DemoAuthOptions> authOptions,
            JwtTokenService tokenService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.Problem(
                    title: "Bad request",
                    detail: "Email and password are required.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var email = request.Email.Trim().ToLowerInvariant();

            var user = authOptions.Value.Users.FirstOrDefault(candidate =>
                string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase));

            if (user is null || user.Password != request.Password)
            {
                return Results.Problem(
                    title: "Unauthorized",
                    detail: "Invalid email or password.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var response = tokenService.CreateToken(user);

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("Login")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            var email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var fullName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var role = user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            return Results.Ok(new CurrentUserResponse
            {
                Email = email,
                FullName = fullName,
                Role = role
            });
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}