using be.DAL;

using Microsoft.EntityFrameworkCore;

namespace be.Services
{
    public class ChatService : IChatService
    {
        private ChatDataContext dbContext;

        public ChatService(ChatDataContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Guid> CreateGroup(string groupName)
        {
            var group = await dbContext.Groups.AddAsync(new Group { GroupName = groupName });
            await dbContext.SaveChangesAsync();
            return group.Entity.GroupID;
        }

        public async Task CreateUserIfNotExist(string? userId, string userName)
        {
            var user = (await dbContext.Users.FindAsync(userId));
            if (user != null)
            {
                return;
            }
            await dbContext.Users.AddAsync(new User { UserID = userId, UserName = userName });
            await dbContext.SaveChangesAsync();
        }

        public async Task<string> GetGroupName(Guid roomId)
        {
            return (await dbContext.Groups.FindAsync(roomId)).GroupName;
        }

        public async Task<List<Message>> GetMessages(Guid groupId)
        {
            return (await dbContext.Messages.Where(m => m.GroupID == groupId).ToListAsync());
        }

        public async Task<string> GetUserName(string userId)
        {
            var user = (await dbContext.Users.FindAsync(userId));
            if (user == null)
            {
                return null;
            }
            return user.UserName;
        }

        public async Task JoinGroup(string userId, Guid groupId)
        {
            await dbContext.UserGroups.AddAsync(new UserGroup { UserID = userId, GroupID = groupId });
            await dbContext.SaveChangesAsync();
        }

        public Task LeaveGroup(string userId, Guid groupId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> SendMessage(string userId, Guid groupId, string message)
        {
            throw new NotImplementedException();
        }

        public async Task SetUserName(string userId, string userName)
        {
            var user = (await dbContext.Users.FindAsync(userId));
            if (user != null)
            {
                user.UserName = userName;
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
