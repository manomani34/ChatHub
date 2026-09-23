using ChatHub.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Web.Areas.User.Controllers;

[Area("User")]
public class HomeController : Controller
{
    private readonly ChatHubApiClient _apiClient;

    public HomeController(ChatHubApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int? channelId)
    {
        var workspaces = await _apiClient.GetWorkspacesAsync();

        var workspace = workspaces.FirstOrDefault();

        if (workspace is null)
        {
            return View(new ChatHubHomeViewModel());
        }

        var channels = await _apiClient.GetChannelsAsync(workspace.Id);

        var selectedChannel = channels.FirstOrDefault();

        if (channelId.HasValue)
        {
            selectedChannel = channels
                .FirstOrDefault(x => x.Id == channelId.Value)
                ?? selectedChannel;
        }

        var messages = new List<MessageDto>();

        if (selectedChannel is not null)
        {
            messages = await _apiClient
                .GetMessagesAsync(selectedChannel.Id);
        }

        var model = new ChatHubHomeViewModel
        {
            Workspace = workspace,
            Channels = channels,
            SelectedChannel = selectedChannel,
            Messages = messages
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(
    int channelId,
    string content)
    {

        Console.WriteLine(
    $"SEND MESSAGE => channelId={channelId}, content={content}");
        if (channelId <= 0)
        {
            return BadRequest();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest();
        }

        await _apiClient.SendMessageAsync(
            senderId: 1,
            channelId: channelId,
            content: content);

        return RedirectToAction(
            nameof(Index),
            new { channelId });
    }

    [HttpPost]
    public async Task<IActionResult> EditMessage(
    int messageId,
    string content)
    {
        if (messageId <= 0)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(content))
            return BadRequest();

        var message =
            await _apiClient.EditMessageAsync(
                messageId,
                userId: 1,
                content: content);

        if (message is null)
            return NotFound();

        return Ok(message);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMessage(
    int messageId)
    {
        if (messageId <= 0)
            return BadRequest();

        var message =
            await _apiClient.DeleteMessageAsync(
                messageId,
                userId: 1);

        if (message is null)
            return NotFound();

        return Ok(message);
    }
}

public class ChatHubHomeViewModel
{
    public WorkspaceDto? Workspace { get; set; }

    public List<ChannelDto> Channels { get; set; }
        = new();

    public ChannelDto? SelectedChannel { get; set; }

    public List<MessageDto> Messages { get; set; }
        = new();
}