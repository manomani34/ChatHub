using ChatHub.Api.Services;
using ChatHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatHub.Api.Hubs;

[Authorize]
public class ChatHubHub : Hub
{
    private readonly IWorkspaceMembershipRepository _membershipRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly UserPresenceTracker _presenceTracker;


    public ChatHubHub(
    IWorkspaceMembershipRepository membershipRepository,
    IConversationRepository conversationRepository,
    IUserRepository userRepository,
    UserPresenceTracker presenceTracker)
    {
        _membershipRepository =
            membershipRepository;

        _conversationRepository =
            conversationRepository;

        _userRepository =
            userRepository;

        _presenceTracker =
            presenceTracker;
    }

    /* =========================================================
       Current User
       ========================================================= */

    private int? GetCurrentUserId()
    {
        var claim =
            Context.User?.FindFirst(
                ClaimTypes.NameIdentifier);

        if (
            claim is null ||
            !int.TryParse(
                claim.Value,
                out var userId) ||
            userId <= 0)
        {
            return null;
        }

        return userId;
    }


    /* =========================================================
       Join Channel
       ========================================================= */

    public async Task JoinChannel(int channelId)
    {
        if (channelId <= 0)
        {
            throw new HubException(
                "Invalid channel id.");
        }


        var userId =
            GetCurrentUserId();

        if (userId is null)
        {
            throw new HubException(
                "User is not authenticated.");
        }


        var isMember =
            await _membershipRepository
                .IsMemberOfChannelAsync(
                    channelId,
                    userId.Value);


        if (!isMember)
        {
            throw new HubException(
                "You are not a member of this channel.");
        }


        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetChannelGroupName(channelId));
    }

    public async Task JoinConversation(int conversationId)
    {
        if (conversationId <= 0)
            throw new HubException("Invalid conversation id.");

        var userId = GetCurrentUserId();

        if (userId is null)
            throw new HubException("User is not authenticated.");

        var isMember =
    await _conversationRepository
        .IsConversationMemberAsync(
            conversationId,
            userId.Value);

        if (!isMember)
            throw new HubException(
                "You are not a member of this conversation.");

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"conversation-{conversationId}");
    }


    /* =========================================================
       Leave Channel
       ========================================================= */

    public async Task LeaveChannel(int channelId)
    {
        if (channelId <= 0)
        {
            throw new HubException(
                "Invalid channel id.");
        }


        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetChannelGroupName(channelId));
    }


    /* =========================================================
       Group Name
       ========================================================= */

    private static string GetChannelGroupName(
        int channelId)
    {
        return $"channel-{channelId}";
    }

    /* =========================================================
   Typing Indicator
   ========================================================= */

    public async Task StartTyping(int conversationId)
    {
        if (conversationId <= 0)
            throw new HubException(
                "Invalid conversation id.");

        var userId =
            GetCurrentUserId();

        if (userId is null)
            throw new HubException(
                "User is not authenticated.");

        var isMember =
            await _conversationRepository
                .IsConversationMemberAsync(
                    conversationId,
                    userId.Value);

        if (!isMember)
            throw new HubException(
                "You are not a member of this conversation.");

        await Clients
            .OthersInGroup(
                $"conversation-{conversationId}")
            .SendAsync(
                "UserTyping",
                new
                {
                    conversationId,
                    userId = userId.Value
                });
    }


    public async Task StopTyping(int conversationId)
    {
        if (conversationId <= 0)
            throw new HubException(
                "Invalid conversation id.");

        var userId =
            GetCurrentUserId();

        if (userId is null)
            throw new HubException(
                "User is not authenticated.");

        var isMember =
            await _conversationRepository
                .IsConversationMemberAsync(
                    conversationId,
                    userId.Value);

        if (!isMember)
            throw new HubException(
                "You are not a member of this conversation.");

        await Clients
            .OthersInGroup(
                $"conversation-{conversationId}")
            .SendAsync(
                "UserStoppedTyping",
                new
                {
                    conversationId,
                    userId = userId.Value
                });
    }

    /* =========================================================
   Disconnected
   ========================================================= */

    /* =========================================================
   Connected
   ========================================================= */

    public override async Task OnConnectedAsync()
    {
        var userId =
            GetCurrentUserId();

        if (userId is not null)
        {
            var becameOnline =
                _presenceTracker.Connect(
                    userId.Value);

            if (becameOnline)
            {
                Console.WriteLine(
                    $"User {userId.Value} is now ONLINE.");

                await BroadcastPresenceAsync(
                    userId.Value,
                    true);
            }
        }

        await base.OnConnectedAsync();
    }


    /* =========================================================
       Disconnected
       ========================================================= */

    public override async Task OnDisconnectedAsync(
    Exception? exception)
    {
        var userId =
            GetCurrentUserId();

        if (userId is not null)
        {
            var becameOffline =
                _presenceTracker.Disconnect(
                    userId.Value);

            if (becameOffline)
            {
                var lastSeenAt =
                    DateTime.UtcNow;

                await _userRepository
                    .UpdateLastSeenAsync(
                        userId.Value,
                        lastSeenAt);

                Console.WriteLine(
                    $"User {userId.Value} is now OFFLINE.");

                await BroadcastPresenceAsync(
                    userId.Value,
                    false,
                    lastSeenAt);
            }
            else
            {
                Console.WriteLine(
                    $"User {userId.Value} still has active connections.");
            }
        }

        await base.OnDisconnectedAsync(
            exception);
    }

    /* =========================================================
   Conversation Presence
   ========================================================= */

    public async Task<object?> GetConversationPresence(
        int conversationId)
    {
        if (conversationId <= 0)
            throw new HubException(
                "Invalid conversation id.");

        var currentUserId =
            GetCurrentUserId();

        if (currentUserId is null)
            throw new HubException(
                "User is not authenticated.");

        var isMember =
            await _conversationRepository
                .IsConversationMemberAsync(
                    conversationId,
                    currentUserId.Value);

        if (!isMember)
            throw new HubException(
                "You are not a member of this conversation.");

        var memberUserIds =
            await _conversationRepository
                .GetMemberUserIdsAsync(
                    conversationId);

        var otherUserId =
            memberUserIds
                .FirstOrDefault(
                    x => x != currentUserId.Value);

        if (otherUserId <= 0)
            return null;

        var otherUser =
            await _userRepository
                .GetByIdAsync(
                    otherUserId);

        if (otherUser is null)
            return null;

        return new
        {
            userId = otherUser.Id,

            isOnline =
                _presenceTracker
                    .IsOnline(otherUser.Id),

            lastSeenAt =
                otherUser.LastSeenAt
        };
    }

    /* =========================================================
   Broadcast Presence
   ========================================================= */

    private async Task BroadcastPresenceAsync(
    int userId,
    bool isOnline,
    DateTime? lastSeenAt = null)
    {
        if (userId <= 0)
            return;

        var conversations =
            await _conversationRepository
                .GetDirectConversationsAsync(
                    userId);

        foreach (var conversation in conversations)
        {
            foreach (var member in conversation.Members)
            {
                if (member.UserId == userId)
                    continue;

                await Clients
                    .Group($"user-{member.UserId}")
                    .SendAsync(
                        "PresenceChanged",
                        new
                        {
                            userId,
                            conversationId =
                                conversation.Id,
                            isOnline,
                            lastSeenAt
                        });
            }
        }
    }

    /* =========================================================
   Online Users
   ========================================================= */

    public List<int> GetOnlineUserIds()
    {
        var userId =
            GetCurrentUserId();

        if (userId is null)
        {
            throw new HubException(
                "User is not authenticated.");
        }

        return _presenceTracker
            .GetOnlineUserIds();
    }


    public async Task JoinUser()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            throw new HubException("User is not authenticated.");

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"user-{userId.Value}");
    }
}