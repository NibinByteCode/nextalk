using Microsoft.AspNetCore.Identity;
using NexTalk.Application.Abstractions;
using NexTalk.Application.DTOs;
using NexTalk.Domain.Entities;

namespace NexTalk.Application.Services;

public sealed class AuthService(
    IChatStore store,
    IPasswordHasher<ChatUser> passwordHasher,
    IJwtTokenService tokenService)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();
        if (userName.Length is < 2 or > 80) throw new ArgumentException("Username must be between 2 and 80 characters.");
        if (request.Password.Length < 10) throw new ArgumentException("Password must contain at least 10 characters.");
        if (await store.FindUserByEmailAsync(email, ct) is not null) throw new InvalidOperationException("Email is already registered.");

        var user = new ChatUser { Email = email, UserName = userName };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        await store.CreateUserAsync(user, ct);
        return ToResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await store.FindUserByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid email or password.");
        return ToResponse(user);
    }

    private AuthResponse ToResponse(ChatUser user) => new(tokenService.CreateToken(user), user.Id, user.UserName, user.Email);
}
