using Google.Cloud.Firestore;
using NexTalk.Application.Abstractions;
using NexTalk.Domain.Entities;

namespace NexTalk.Infrastructure.Firebase;

public sealed class FirestoreChatStore(FirestoreDb db) : IChatStore
{
    private CollectionReference Users => db.Collection("users");
    private CollectionReference Conversations => db.Collection("conversations");
    private CollectionReference Messages => db.Collection("messages");

    public async Task<ChatUser?> FindUserByEmailAsync(string email, CancellationToken ct = default)
    {
        var result = await Users.WhereEqualTo("Email", email.ToLowerInvariant()).Limit(1).GetSnapshotAsync(ct);
        return result.Documents.Count == 0 ? null : result.Documents[0].ConvertTo<ChatUser>();
    }
    public async Task<ChatUser?> FindUserByIdAsync(string id, CancellationToken ct = default)
    {
        var snapshot = await Users.Document(id).GetSnapshotAsync(ct);
        return snapshot.Exists ? snapshot.ConvertTo<ChatUser>() : null;
    }
    public Task CreateUserAsync(ChatUser user, CancellationToken ct = default) =>
        Users.Document(user.Id).SetAsync(user, cancellationToken: ct);
    public Task CreateConversationAsync(ChatConversation conversation, CancellationToken ct = default) =>
        Conversations.Document(conversation.Id).SetAsync(conversation, cancellationToken: ct);
    public async Task<ChatConversation?> FindConversationAsync(string id, CancellationToken ct = default)
    {
        var snapshot = await Conversations.Document(id).GetSnapshotAsync(ct);
        return snapshot.Exists ? snapshot.ConvertTo<ChatConversation>() : null;
    }
    public async Task<IReadOnlyList<ChatConversation>> GetUserConversationsAsync(string userId, CancellationToken ct = default)
    {
        var result = await Conversations.WhereArrayContains("MemberIds", userId).GetSnapshotAsync(ct);
        return result.Documents.Select(x => x.ConvertTo<ChatConversation>()).Where(x => x.Kind != ConversationKind.Public)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ToList();
    }
    public async Task<IReadOnlyList<ChatConversation>> GetPublicRoomsAsync(CancellationToken ct = default)
    {
        var result = await Conversations.WhereEqualTo("Kind", (int)ConversationKind.Public).GetSnapshotAsync(ct);
        return result.Documents.Select(x => x.ConvertTo<ChatConversation>()).OrderBy(x => x.Name).ToList();
    }
    public async Task AddMemberAsync(string conversationId, string userId, CancellationToken ct = default)
    {
        var doc = Conversations.Document(conversationId);
        await db.RunTransactionAsync(async transaction =>
        {
            var snapshot = await transaction.GetSnapshotAsync(doc);
            if (!snapshot.Exists) throw new InvalidOperationException("Conversation not found.");
            var conversation = snapshot.ConvertTo<ChatConversation>();
            if (!conversation.MemberIds.Contains(userId))
            {
                conversation.MemberIds.Add(userId);
                conversation.UpdatedAt = DateTimeOffset.UtcNow;
                transaction.Set(doc, conversation);
            }
        }, cancellationToken: ct);
    }
    public async Task<bool> IsConversationMemberAsync(string conversationId, string userId, CancellationToken ct = default)
    {
        var conversation = await FindConversationAsync(conversationId, ct);
        return conversation is not null && (conversation.Kind == ConversationKind.Public || conversation.MemberIds.Contains(userId));
    }
    public async Task AddMessageAsync(ChatMessage message, CancellationToken ct = default)
    {
        await Messages.Document(message.Id).SetAsync(message, cancellationToken: ct);
        await Conversations.Document(message.ConversationId).UpdateAsync("UpdatedAt", message.SentAt, cancellationToken: ct);
    }
    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string conversationId, int take = 50, CancellationToken ct = default)
    {
        var result = await Messages.WhereEqualTo("ConversationId", conversationId).OrderByDescending("SentAt")
            .Limit(Math.Clamp(take, 1, 100)).GetSnapshotAsync(ct);
        return result.Documents.Select(x => x.ConvertTo<ChatMessage>()).OrderBy(x => x.SentAt).ToList();
    }
}
