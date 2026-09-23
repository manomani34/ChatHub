using ChatHub.Domain.Entities;
using ChatHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Persistence.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        // =========================
        // Workspace
        // =========================

        if (!await db.Workspaces.AnyAsync())
        {
            db.Workspaces.Add(new Workspace
            {
                Name = "ChatHub",
                Description = "ChatHub default workspace",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var workspace = await db.Workspaces
            .FirstAsync(x => x.Name == "ChatHub");


        // =========================
        // Users
        // =========================

        var defaultUsers = new[]
        {
            new User
            {
                UserName = "ali",
                Email = "ali@chathub.local",
                DisplayName = "علی",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            new User
            {
                UserName = "reza",
                Email = "reza@chathub.local",
                DisplayName = "رضا",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            new User
            {
                UserName = "sara",
                Email = "sara@chathub.local",
                DisplayName = "سارا",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            new User
            {
                UserName = "mohammad",
                Email = "mohammad@chathub.local",
                DisplayName = "محمد",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var user in defaultUsers)
        {
            var existingUser = await db.Users
                .FirstOrDefaultAsync(x => x.UserName == user.UserName);

            if (existingUser is null)
            {
                db.Users.Add(user);
            }
        }

        await db.SaveChangesAsync();


        // =========================
        // Workspace Members
        // =========================

        var users = await db.Users
            .Where(x =>
                x.UserName == "ali" ||
                x.UserName == "reza" ||
                x.UserName == "sara" ||
                x.UserName == "mohammad")
            .ToListAsync();

        foreach (var user in users)
        {
            var existingMember = await db.WorkspaceMembers
                .FirstOrDefaultAsync(x =>
                    x.WorkspaceId == workspace.Id &&
                    x.UserId == user.Id);

            if (existingMember is null)
            {
                db.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = workspace.Id,
                    UserId = user.Id,
                    IsOwner = user.UserName == "ali",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();


        // =========================
        // Channels
        // =========================

        var defaultChannels = new[]
        {
            new Channel
            {
                WorkspaceId = workspace.Id,
                Name = "general",
                SortOrder = 1,
                Description = "General discussions",
                IsPrivate = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            new Channel
            {
                WorkspaceId = workspace.Id,
                Name = "development",
                SortOrder = 2,
                Description = "Development discussions",
                IsPrivate = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            new Channel
            {
                WorkspaceId = workspace.Id,
                Name = "team",
                SortOrder = 3,
                Description = "Team discussions",
                IsPrivate = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            new Channel
            {
                WorkspaceId = workspace.Id,
                Name = "announcements",
                SortOrder = 4,
                Description = "Important announcements",
                IsPrivate = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var channel in defaultChannels)
        {
            var existingChannel = await db.Channels
                .FirstOrDefaultAsync(x =>
                    x.WorkspaceId == workspace.Id &&
                    x.Name == channel.Name);

            if (existingChannel is null)
            {
                db.Channels.Add(channel);
            }
            else
            {
                existingChannel.SortOrder = channel.SortOrder;
                existingChannel.Description = channel.Description;
                existingChannel.IsPrivate = channel.IsPrivate;
                existingChannel.IsActive = channel.IsActive;
            }
        }

        await db.SaveChangesAsync();


        // =========================
        // Messages
        // =========================

        var generalChannel = await db.Channels
            .FirstAsync(x =>
                x.WorkspaceId == workspace.Id &&
                x.Name == "general");

        var ali = await db.Users
            .FirstAsync(x => x.UserName == "ali");

        var reza = await db.Users
            .FirstAsync(x => x.UserName == "reza");

        var sara = await db.Users
            .FirstAsync(x => x.UserName == "sara");

        var defaultMessages = new[]
        {
            new Message
            {
                SenderId = ali.Id,
                ChannelId = generalChannel.Id,
                Content = "سلام به همه 👋",
                IsEdited = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            },

            new Message
            {
                SenderId = reza.Id,
                ChannelId = generalChannel.Id,
                Content = "سلام علی، خوش اومدی.",
                IsEdited = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-8)
            },

            new Message
            {
                SenderId = sara.Id,
                ChannelId = generalChannel.Id,
                Content = "امروز روی بخش پیام‌ها کار می‌کنیم.",
                IsEdited = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        foreach (var message in defaultMessages)
        {
            var exists = await db.Messages
                .AnyAsync(x =>
                    x.ChannelId == generalChannel.Id &&
                    x.SenderId == message.SenderId &&
                    x.Content == message.Content);

            if (!exists)
            {
                db.Messages.Add(message);
            }
        }

        await db.SaveChangesAsync();
    }
}