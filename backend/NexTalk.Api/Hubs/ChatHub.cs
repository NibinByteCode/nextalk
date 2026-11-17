using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NexTalk.Application.Services;

namespace NexTalk.Api.Hubs;

[Authorize]
public sealed class ChatHub(ChatService chat) : Hub
{
    private string UserId => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new HubException("Unauthenticated.");
    private static string GroupName(string id) => $"conversation:{id}";

    public async Task JoinConversation(string conversationId)
    {
        try
        {
            await chat.JoinConversationAsync(UserId, conversationId);
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId));
        }
        catch (Exception ex) when (ex is KeyNotFoundException or UnauthorizedAccessException) { throw new HubException(ex.Message); }
    }

    public Task LeaveConversation(string conversationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(conversationId));

    public async Task SendMessage(string conversationId, string content)
    {
        try
        {
            var message = await chat.SaveMessageAsync(UserId, conversationId, content);
            await Clients.Group(GroupName(conversationId)).SendAsync("ReceiveMessage", message);
            var recipients = (await chat.GetConversationMemberIdsAsync(UserId, conversationId))
                .Where(id => id != UserId)
                .ToList();
            if (recipients.Count > 0)
                await Clients.Users(recipients).SendAsync("MessageNotification", message);
        }
        catch (Exception ex) when (ex is ArgumentException or UnauthorizedAccessException or KeyNotFoundException) { throw new HubException(ex.Message); }
    }
}
