namespace NexTalk.Domain.Entities;

public enum ConversationKind
{
    Private = 1,
    Group = 2,
    Public = 3
}

public sealed class ChatConversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public ConversationKind Kind { get; set; }
    public string CreatedByUserId { get; set; } = "";
    public List<string> MemberIds { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
