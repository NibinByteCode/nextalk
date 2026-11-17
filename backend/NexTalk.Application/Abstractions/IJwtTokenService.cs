using NexTalk.Domain.Entities;

namespace NexTalk.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateToken(ChatUser user);
}
