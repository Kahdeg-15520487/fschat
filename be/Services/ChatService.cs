using be.DAL;

using Microsoft.EntityFrameworkCore;
using CrypticWizard.RandomWordGenerator;
using static CrypticWizard.RandomWordGenerator.WordGenerator;

namespace be.Services
{
    public class ChatService : IChatService
    {
        private ChatDataContext dbContext;

        private static List<PartOfSpeech> pattern = new List<PartOfSpeech>() { PartOfSpeech.adv, PartOfSpeech.adj, PartOfSpeech.noun };


        public ChatService(ChatDataContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Guid> CreateGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                groupName = new WordGenerator().GetPattern(pattern, ' ');
            }
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

        public async Task<List<MessageObject>> GetMessages(Guid groupId)
        {
            var result = await dbContext.Messages.Include(m => m.Sender).Where(m => m.GroupID == groupId).OrderBy(m => m.Timestamp).ToListAsync();
            return result.Select(m => new MessageObject(m.GroupID.ToString(), m.SenderID, m.Content) { user = m.Sender.UserName }).ToList();
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
            if (!await dbContext.UserGroups.AnyAsync(ug => ug.UserID == userId && ug.GroupID == groupId))
            {
                await dbContext.UserGroups.AddAsync(new UserGroup { UserID = userId, GroupID = groupId });
                await dbContext.SaveChangesAsync();
            }
        }

        public Task LeaveGroup(string userId, Guid groupId)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> SendMessage(string userId, Guid groupId, string message)
        {
            var result = await dbContext.Messages.AddAsync(new Message { SenderID = userId, GroupID = groupId, Content = message, Timestamp = DateTimeOffset.UtcNow });
            await dbContext.SaveChangesAsync();
            return result.Entity.MessageID;
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

        public async Task<Guid> GetGroupByName(string roomName)
        {
            return await dbContext.Groups.Where(g => g.GroupName == roomName).Select(g => g.GroupID).FirstOrDefaultAsync();
        }

        public async Task<MessageObject> GetMessage(Guid messageId)
        {
            return await dbContext.Messages.FirstOrDefaultAsync(m => m.MessageID == messageId) switch
            {
                Message m => new MessageObject(m.GroupID.ToString(), m.SenderID, m.Content) { user = m.Sender.UserName },
                _ => null
            };
        }
    }
}
