using ChatHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatHub.Infrastructure.Persistence.Configurations;

public class UserConversationReadStateConfiguration
    : IEntityTypeConfiguration<UserConversationReadState>
{
    public void Configure(
        EntityTypeBuilder<UserConversationReadState> builder)
    {
        builder.ToTable(
            "UserConversationReadStates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ConversationId)
            .IsRequired();

        builder.Property(x => x.LastReadMessageId)
            .IsRequired(false);

        builder.Property(x => x.LastReadAt)
            .IsRequired(false);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Conversation)
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(
                x => new
                {
                    x.UserId,
                    x.ConversationId
                })
            .IsUnique();
    }
}