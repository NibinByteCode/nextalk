using NexTalk.Domain.Entities;

namespace NexTalk.Application.Abstractions;

public interface IChatStore
{
    Task<ChatUser?> FindUserByEmailAsync(string email, CancellationToken ct = default);
    Task<ChatUser?> FindUserByIdAsync(string id, CancellationToken ct = default);
    Task CreateUserAsync(ChatUser user, CancellationToken ct = default);
    Task CreateConversationAsync(ChatConversation conversation, CancellationToken ct = default);
    Task<ChatConversation?> FindConversationAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<ChatConversation>> GetUserConversationsAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<ChatConversation>> GetPublicRoomsAsync(CancellationToken ct = default);
    Task AddMemberAsync(string conversationId, string userId, CancellationToken ct = default);
    Task<bool> IsConversationMemberAsync(string conversationId, string userId, CancellationToken ct = default);
    Task AddMessageAsync(ChatMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string conversationId, int take = 50, CancellationToken ct = default);
}
