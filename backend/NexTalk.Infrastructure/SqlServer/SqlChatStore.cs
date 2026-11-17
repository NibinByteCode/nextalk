using Microsoft.EntityFrameworkCore;
using NexTalk.Application.Abstractions;
using NexTalk.Domain.Entities;

namespace NexTalk.Infrastructure.SqlServer;

public sealed class SqlChatStore(NexTalkDbContext db) : IChatStore
{
    public Task<ChatUser?> FindUserByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(x => x.Email == email.ToLowerInvariant(), ct);
    public Task<ChatUser?> FindUserByIdAsync(string id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<ChatUser>> SearchUsersAsync(string query, CancellationToken ct = default)
    {
        query = query.Trim().ToLowerInvariant();
        return await db.Users.Where(x => x.UserName.ToLower().Contains(query) || x.Email.Contains(query))
            .OrderBy(x => x.UserName).Take(20).ToListAsync(ct);
    }
    public async Task CreateUserAsync(ChatUser user, CancellationToken ct = default) { db.Users.Add(user); await db.SaveChangesAsync(ct); }
    public async Task CreateConversationAsync(ChatConversation conversation, CancellationToken ct = default) { db.Conversations.Add(conversation); await db.SaveChangesAsync(ct); }
    public Task<ChatConversation?> FindConversationAsync(string id, CancellationToken ct = default) =>
        db.Conversations.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<ChatConversation?> FindPrivateConversationAsync(string firstUserId, string secondUserId, CancellationToken ct = default)
    {
        var conversations = await db.Conversations
            .Where(x => x.Kind == ConversationKind.Private)
            .ToListAsync(ct);
        return conversations.FirstOrDefault(x =>
            x.MemberIds.Count == 2 &&
            x.MemberIds.Contains(firstUserId) &&
            x.MemberIds.Contains(secondUserId));
    }
    public async Task<IReadOnlyList<ChatConversation>> GetUserConversationsAsync(string userId, CancellationToken ct = default)
    {
        var conversations = await db.Conversations
            .Where(x => x.Kind != ConversationKind.Public)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync(ct);

        return conversations.Where(x => x.MemberIds.Contains(userId)).ToList();
    }
    public async Task<IReadOnlyList<ChatConversation>> GetPublicRoomsAsync(CancellationToken ct = default) =>
        await db.Conversations.Where(x => x.Kind == ConversationKind.Public).OrderBy(x => x.Name).ToListAsync(ct);
    public async Task AddMemberAsync(string conversationId, string userId, CancellationToken ct = default)
    {
        var conversation = await db.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId, ct)
            ?? throw new InvalidOperationException("Conversation not found.");
        if (!conversation.MemberIds.Contains(userId))
        {
            conversation.MemberIds.Add(userId);
            conversation.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }
    public async Task<bool> IsConversationMemberAsync(string conversationId, string userId, CancellationToken ct = default)
    {
        var conversation = await FindConversationAsync(conversationId, ct);
        return conversation is not null && (conversation.Kind == ConversationKind.Public || conversation.MemberIds.Contains(userId));
    }
    public async Task AddMessageAsync(ChatMessage message, CancellationToken ct = default)
    {
        db.Messages.Add(message);
        var conversation = await db.Conversations.FirstOrDefaultAsync(x => x.Id == message.ConversationId, ct);
        if (conversation is not null) conversation.UpdatedAt = message.SentAt;
        await db.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string conversationId, int take = 50, CancellationToken ct = default) =>
        await db.Messages.Where(x => x.ConversationId == conversationId).OrderByDescending(x => x.SentAt)
            .Take(Math.Clamp(take, 1, 100)).OrderBy(x => x.SentAt).ToListAsync(ct);
}
