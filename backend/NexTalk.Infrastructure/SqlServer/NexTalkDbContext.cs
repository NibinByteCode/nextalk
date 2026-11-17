using Microsoft.EntityFrameworkCore;
using NexTalk.Domain.Entities;

namespace NexTalk.Infrastructure.SqlServer;

public sealed class NexTalkDbContext(DbContextOptions<NexTalkDbContext> options) : DbContext(options)
{
    public DbSet<ChatUser> Users => Set<ChatUser>();
    public DbSet<ChatConversation> Conversations => Set<ChatConversation>();
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(320).IsRequired();
            e.Property(x => x.UserName).HasMaxLength(80).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
        });
        modelBuilder.Entity<ChatConversation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Kind).HasConversion<string>();
            e.Property(x => x.MemberIds).HasColumnType("nvarchar(max)");
        });
        modelBuilder.Entity<ChatMessage>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Content).HasMaxLength(4000).IsRequired();
            e.HasIndex(x => new { x.ConversationId, x.SentAt });
        });
    }
}
