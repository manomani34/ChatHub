using ChatHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatHub.Infrastructure.Persistence.Configurations;

public class MessageReactionConfiguration
    : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(
        EntityTypeBuilder<MessageReaction> builder)
    {
        builder.ToTable("MessageReactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Emoji)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasOne(x => x.Message)
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.MessageId,
            x.UserId,
            x.Emoji
        })
        .IsUnique();
    }
}