using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

public class MessageObject
{
    public string user { get; set; }
    public string message { get; set; }
    public string room { get; set; }
    public MessageObject(string room, string user, string message)
    {
        this.room = room;
        this.user = user;
        this.message = message;
    }
}

public class ChatHub : Hub
{
    public async Task JoinRoom(MessageObject msg)
    {
        var accessToken = Context.GetHttpContext().Request.Query["access_token"];

        msg.room = Guid.NewGuid().ToString();
        await Groups.AddToGroupAsync(Context.ConnectionId, msg.room);
        await Clients.All.SendAsync("new user joined", new MessageObject(msg.room, msg.user, "has joined the room."));
        Console.WriteLine($"{msg.user} joined {msg.room}");
    }

    public async Task LeaveRoom(MessageObject msg)
    {
        await Clients.All.SendAsync("left room", new MessageObject(msg.room, msg.user, "has left the room."));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, msg.room);
        Console.WriteLine($"{msg.user} left {msg.room}");
    }

    public async Task SendMessage(MessageObject msg)
    {
        await Clients.Group(msg.room).SendAsync("new message", msg);
        Console.WriteLine($"Message from {msg.user} in {msg.room}: {msg.message}");
    }
}
