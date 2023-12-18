using be.DAL;

namespace be.Services
{
    public class ChatService : IChatService
    {
        public Task<Guid> CreateGroup(string groupName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Message>> GetMessages(Guid groupId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserName(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> JoinGroup(Guid userId, Guid groupId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> LeaveGroup(Guid userId, Guid groupId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> SendMessage(Guid userId, Guid groupId, string message)
        {
            throw new NotImplementedException();
        }

        public Task SetUserName(Guid userId, string userName)
        {
            throw new NotImplementedException();
        }
    }
}
