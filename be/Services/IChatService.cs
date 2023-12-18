using be.DAL;

namespace be.Services
{
    public interface IChatService
    {
        Task SetUserName(Guid userId, string userName);
        Task<string> GetUserName(Guid userId);
        Task<Guid> CreateGroup(string groupName);
        Task<Guid> JoinGroup(Guid userId, Guid groupId);
        Task<Guid> LeaveGroup(Guid userId, Guid groupId);
        Task<Guid> SendMessage(Guid userId, Guid groupId, string message);
        Task<List<Message>> GetMessages(Guid groupId);
    }
}
