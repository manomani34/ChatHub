using ChatHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<MessageReaction> MessageReactions { get; set; }
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Workspace> Workspaces => Set<Workspace>();

    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    public DbSet<Channel> Channels => Set<Channel>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<UserConversationReadState> UserConversationReadStates => Set<UserConversationReadState>();    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}