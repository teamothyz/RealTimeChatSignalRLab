using Microsoft.EntityFrameworkCore;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;

namespace RealTimeChatSignalRLab.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDBContext dbcontext;

        public MessageRepository(ChatDBContext context)
        {
            dbcontext = context;
        }

        public async Task<List<Tuple<User, Message>>> GetGroupMessages(int pageIndex, Guid userId, Guid groupId, long? offsetTime)
        {
            offsetTime ??= DateTime.UtcNow.AddTicks(1).Ticks;
            var existed = await dbcontext.UserGroups.AnyAsync(g => g.UserId == userId && g.GroupId == groupId);
            if (!existed)
                throw new Exception("The user does not join this group");
            var query = from message in dbcontext.Messages
                        join user in dbcontext.Users on message.Sender equals user.Id
                        where message.Reciever == groupId 
                        && message.RecieverType == RecieverType.Group
                        && message.SendTime <= offsetTime
                        orderby message.SendTime descending
                        select Tuple.Create(user, message);
            return await query.Skip((pageIndex - 1) * 10).Take(10).ToListAsync();
        }

        public async Task<List<Tuple<User, Message?, bool>>> GetUserChat(Guid userId, long? offsetTime)
        {
            offsetTime ??= DateTime.UtcNow.AddTicks(1).Ticks;
            var query = from message in dbcontext.Messages
                        join sender in dbcontext.Users on message.Sender equals sender.Id
                        join reciever in dbcontext.Users on message.Reciever equals reciever.Id

                        let id1 = sender.Id.CompareTo(reciever.Id) < 0 ? sender.Id : reciever.Id
                        let id2 = sender.Id.CompareTo(reciever.Id) < 0 ? reciever.Id : sender.Id

                        where (sender.Id == userId || reciever.Id == userId) 
                        && message.RecieverType == RecieverType.User

                        group new { sender, reciever, message } by new { id1, id2 } into messageGroup
                        select messageGroup.OrderByDescending(x => x.message.SendTime)
                        .Select(x => Tuple.Create(x.sender, x.reciever, x.message)).First();

            var messagesInfo = (await query.ToListAsync())
                .Where(user => (user.Item3?.SendTime ?? 0) <= offsetTime)
                .OrderByDescending(info => info.Item3.SendTime)
                .Take(5);
            var users = new List<Tuple<User, Message?, bool>>();
            foreach (var info in messagesInfo)
            {
                var lastMessage = info.Item3;
                var isUnread = lastMessage != null && (lastMessage.Reciever == userId && !lastMessage.IsRead);
                if (info.Item1.Id != userId)
                    users.Add(Tuple.Create(info.Item1, lastMessage, isUnread));
                else
                    users.Add(Tuple.Create(info.Item2, lastMessage, isUnread));
            }
            return users;
        }

        public async Task<List<Tuple<User, Message>>> GetUserMessages(int pageIndex, Guid userId1, Guid userId2, long? offsetTime)
        {
            offsetTime ??= DateTime.UtcNow.AddTicks(1).Ticks;
            var query = from message in dbcontext.Messages
                        join sender in dbcontext.Users on message.Sender equals sender.Id
                        join reciever in dbcontext.Users on message.Reciever equals reciever.Id
                        where ((message.Sender == userId1 && message.Reciever == userId2) 
                        || (message.Sender == userId2 && message.Reciever == userId1))
                        && message.SendTime <= offsetTime
                        orderby message.SendTime descending
                        select Tuple.Create(sender, message);
            return await query.Skip((pageIndex - 1) * 10).Take(10).ToListAsync();
        }

        public async Task Send(Message message)
        {
            await dbcontext.Messages.AddAsync(message);
            await dbcontext.SaveChangesAsync();
        }

        public async Task ReadMessage(Guid sender, Guid reciever)
        {
            var query = from message in dbcontext.Messages
                        where (message.Reciever == reciever && message.Sender == sender)
                        orderby message.SendTime descending
                        select message;
            var mess = query.FirstOrDefault();
            if (mess != null)
            {
                mess.IsRead = true;
                dbcontext.Messages.Update(mess);
                await dbcontext.SaveChangesAsync();
            }
        }

        private async Task<Message?> GetLastMessage(Guid userId1, Guid userId2)
        {
            var query = from message in dbcontext.Messages
                        where (message.Reciever == userId1 && message.Sender == userId2)
                        || (message.Reciever == userId2 && message.Sender == userId1)
                        orderby message.SendTime descending
                        select message;
            return await query.FirstOrDefaultAsync();
        }
    }
}
