public class MessageObject
{
    public MessageObject(string roomId, string userId, string message)
    {
        this.roomId = roomId;
        this.userId = userId;
        this.message = message;
    }

    public string userId { get; set; }
    public string user { get; set; }
    public string messageId { get; set; }
    public string message { get; set; }
    public string roomId { get; set; }
    public string room { get; set; }
}
