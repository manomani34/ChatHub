using ChatHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatHub.Infrastructure.Persistence.Configurations;

public class MessageConfiguration
    : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(10000);

        builder.HasOne(x => x.Sender)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Channel)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Conversation)
     .WithMany(x => x.Messages)
     .HasForeignKey(x => x.ConversationId)
     .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ParentMessage)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.ChannelId,
            x.CreatedAt
        });

        builder.HasIndex(x => new
        {
            x.ConversationId,
            x.CreatedAt
        });
    }
}