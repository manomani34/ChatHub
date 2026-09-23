using Microsoft.AspNetCore.SignalR;

namespace ChatHub.Api.Hubs;

public class ChatHubHub : Hub
{
    public async Task JoinChannel(int channelId)
    {
        if (channelId <= 0)
        {
            throw new HubException("Invalid channel id.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetChannelGroupName(channelId));
    }

    public async Task LeaveChannel(int channelId)
    {
        if (channelId <= 0)
        {
            throw new HubException("Invalid channel id.");
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetChannelGroupName(channelId));
    }

    private static string GetChannelGroupName(int channelId)
    {
        return $"channel-{channelId}";
    }
}