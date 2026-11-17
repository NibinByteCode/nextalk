using NexTalk.Application.Abstractions;
using NexTalk.Application.DTOs;
using NexTalk.Domain.Entities;

namespace NexTalk.Application.Services;

public sealed class ChatService(IChatStore store)
{
    public async Task<ConversationDto> CreateConversationAsync(string creatorId, CreateConversationRequest request, CancellationToken ct = default)
    {
        var members = request.MemberIds.Append(creatorId).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        if (request.Kind == ConversationKind.Private && members.Count != 2) throw new ArgumentException("A private conversation must have exactly two members.");
        if (request.Kind == ConversationKind.Group && members.Count < 2) throw new ArgumentException("A group must have at least two members.");
        if (request.Kind == ConversationKind.Public) throw new ArgumentException("Use the public-room endpoint.");
        foreach (var memberId in members)
            if (await store.FindUserByIdAsync(memberId, ct) is null) throw new ArgumentException($"User '{memberId}' does not exist.");

        var name = request.Kind == ConversationKind.Private ? "Private conversation" : request.Name.Trim();
        if (request.Kind == ConversationKind.Group && name.Length is < 2 or > 120) throw new ArgumentException("Group name must be between 2 and 120 characters.");
        var conversation = new ChatConversation { Kind = request.Kind, Name = name, CreatedByUserId = creatorId, MemberIds = members };
        await store.CreateConversationAsync(conversation, ct);
        return ToDto(conversation);
    }

    public async Task<ConversationDto> CreatePublicRoomAsync(string creatorId, string name, CancellationToken ct = default)
    {
        name = name.Trim();
        if (name.Length is < 2 or > 120) throw new ArgumentException("Room name must be between 2 and 120 characters.");
        var room = new ChatConversation { Kind = ConversationKind.Public, Name = name, CreatedByUserId = creatorId };
        await store.CreateConversationAsync(room, ct);
        return ToDto(room);
    }

    public async Task<IReadOnlyList<ConversationDto>> GetMyConversationsAsync(string userId, CancellationToken ct = default) =>
        (await store.GetUserConversationsAsync(userId, ct)).Select(ToDto).ToList();

    public async Task<IReadOnlyList<ConversationDto>> GetPublicRoomsAsync(CancellationToken ct = default) =>
        (await store.GetPublicRoomsAsync(ct)).Select(ToDto).ToList();

    public async Task<IReadOnlyList<MessageDto>> GetMessagesAsync(string userId, string conversationId, int take, CancellationToken ct = default)
    {
        if (!await store.IsConversationMemberAsync(conversationId, userId, ct)) throw new UnauthorizedAccessException("You cannot access this conversation.");
        return (await store.GetMessagesAsync(conversationId, take, ct)).Select(ToDto).ToList();
    }

    public async Task<MessageDto> SaveMessageAsync(string userId, string conversationId, string content, CancellationToken ct = default)
    {
        content = content.Trim();
        if (content.Length == 0 || content.Length > 4000) throw new ArgumentException("Message must contain 1 to 4000 characters.");
        if (!await store.IsConversationMemberAsync(conversationId, userId, ct)) throw new UnauthorizedAccessException("You cannot send messages to this conversation.");
        var user = await store.FindUserByIdAsync(userId, ct) ?? throw new UnauthorizedAccessException("User not found.");
        var message = new ChatMessage { ConversationId = conversationId, SenderId = user.Id, SenderName = user.UserName, Content = content };
        await store.AddMessageAsync(message, ct);
        return ToDto(message);
    }

    public async Task JoinConversationAsync(string userId, string conversationId, CancellationToken ct = default)
    {
        var conversation = await store.FindConversationAsync(conversationId, ct) ?? throw new KeyNotFoundException("Conversation not found.");
        if (conversation.Kind != ConversationKind.Public && !conversation.MemberIds.Contains(userId))
            throw new UnauthorizedAccessException("You are not a member of this conversation.");
    }

    public async Task AddGroupMemberAsync(string requesterId, string conversationId, string newMemberId, CancellationToken ct = default)
    {
        var conversation = await store.FindConversationAsync(conversationId, ct) ?? throw new KeyNotFoundException("Conversation not found.");
        if (conversation.Kind != ConversationKind.Group) throw new ArgumentException("Members can only be added to groups.");
        if (conversation.CreatedByUserId != requesterId) throw new UnauthorizedAccessException("Only the group creator can add members.");
        if (await store.FindUserByIdAsync(newMemberId, ct) is null) throw new KeyNotFoundException("User not found.");
        await store.AddMemberAsync(conversationId, newMemberId, ct);
    }

    private static ConversationDto ToDto(ChatConversation x) => new(x.Id, x.Name, x.Kind, x.MemberIds, x.CreatedAt);
    private static MessageDto ToDto(ChatMessage x) => new(x.Id, x.ConversationId, x.SenderId, x.SenderName, x.Content, x.SentAt);
}
