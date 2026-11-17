using NexTalk.Domain.Entities;

namespace NexTalk.Application.DTOs;

public sealed record CreateConversationRequest(ConversationKind Kind, string Name, List<string> MemberIds);
public sealed record SendMessageRequest(string Content);
public sealed record CreateRoomRequest(string Name);
public sealed record MessageDto(string Id, string ConversationId, string SenderId, string SenderName, string Content, DateTimeOffset SentAt);
public sealed record ConversationDto(string Id, string Name, ConversationKind Kind, List<string> MemberIds, DateTimeOffset CreatedAt);
