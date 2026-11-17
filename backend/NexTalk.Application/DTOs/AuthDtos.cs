namespace NexTalk.Application.DTOs;

public sealed record RegisterRequest(string UserName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string AccessToken, string UserId, string UserName, string Email);
public sealed record UserSummary(string Id, string UserName, string Email);
