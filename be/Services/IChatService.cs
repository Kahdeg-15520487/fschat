using be.DAL;

namespace be.Services
{
    public interface IChatService
    {
        Task SetUserName(string userId, string userName);
        Task<string> GetUserName(string userId);
        Task<Guid> CreateGroup(string groupName);
        Task JoinGroup(string userId, Guid groupId);
        Task LeaveGroup(string userId, Guid groupId);
        Task<Guid> SendMessage(string userId, Guid groupId, string message);
        Task<List<MessageObject>> GetMessages(Guid groupId);
        Task CreateUserIfNotExist(string? userId, string user);
        Task<string> GetGroupName(Guid roomId);
        Task<Guid> GetGroupByName(string roomId);
        Task<MessageObject> GetMessage(Guid messageId);
    }
}
