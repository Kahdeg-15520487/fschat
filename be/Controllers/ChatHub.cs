using be.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService chatService;

    public ChatHub(IChatService chatService)
    {
        this.chatService = chatService;
    }

    public async Task<string> Echo(MessageObject message)
    {
        return "lala";
    }

    public async Task<string> JoinRoom(MessageObject msg)
    {
        msg.userId = Context.User.Identity.Name;
        await chatService.CreateUserIfNotExist(msg.userId, msg.user);
        if (Guid.TryParse(msg.roomId, out var roomId) || roomId == Guid.Empty)
        {
            msg.roomId = (await chatService.CreateGroup(msg.room)).ToString();
        }
        else
        {
            msg.room = await chatService.GetGroupName(roomId);
        }

        await chatService.JoinGroup(msg.userId, Guid.Parse(msg.roomId));
        await Groups.AddToGroupAsync(Context.ConnectionId, msg.roomId);
        msg.message = "has joined the room.";
        await Clients.All.SendAsync("new user joined", msg);
        Console.WriteLine($"<{msg.user}|{msg.userId}> joined <{msg.room}|{msg.roomId}>");
        return msg.roomId;
    }

    public async Task LeaveRoom(MessageObject msg)
    {
        await Clients.All.SendAsync("left room", new MessageObject(msg.roomId, msg.user, "has left the room."));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, msg.roomId);
        Console.WriteLine($"<{msg.user}|{msg.userId}> left <{msg.room}|{msg.roomId}>");
    }

    public async Task SendMessage(MessageObject msg)
    {
        await Clients.Group(msg.roomId).SendAsync("new message", msg);
        Console.WriteLine($"Message from {msg.user} in {msg.room}: {msg.message}");
    }
}
