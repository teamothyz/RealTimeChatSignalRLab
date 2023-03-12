using Microsoft.EntityFrameworkCore;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;
using RealTimeChatSignalRLab.Pagination;

namespace RealTimeChatSignalRLab.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDBContext dbcontext;

        public MessageRepository(ChatDBContext context)
        {
            dbcontext = context;
        }

        public async Task<PaginatedList<Tuple<User, Message>>> GetGroupMessages(int pageIndex, Guid userId, Guid groupId)
        {
            var existed = await dbcontext.UserGroups.AnyAsync(g => g.UserId == userId && g.GroupId == groupId);
            if (!existed)
                throw new Exception("The user does not join this group");
            var query = from message in dbcontext.Messages
                        join user in dbcontext.Users on message.Sender equals user.Id
                        where message.Reciever == groupId && message.RecieverType == RecieverType.Group
                        orderby message.SendTime descending
                        select Tuple.Create(user, message);
            return await PaginatedList<Tuple<User, Message>>.CreateAsync(query, pageIndex, 10);
        }

        public async Task<PaginatedList<Tuple<User, Message?, bool>>> GetUserChat(int pageIndex, Guid userId)
        {
            var query = from message in dbcontext.Messages
                        join sender in dbcontext.Users on message.Sender equals sender.Id
                        join reciever in dbcontext.Users on message.Reciever equals reciever.Id

                        let id1 = sender.Id.CompareTo(reciever.Id) < 0 ? sender.Id : reciever.Id
                        let id2 = sender.Id.CompareTo(reciever.Id) < 0 ? reciever.Id : sender.Id

                        where (sender.Id == userId || reciever.Id == userId) && message.RecieverType == RecieverType.User

                        group new { sender, reciever } by new { id1, id2 } into g
                        select Tuple.Create(g.First().sender, g.First().reciever);

            var userPairs = await PaginatedList<Tuple<User, User>>.CreateAsync(query, pageIndex, 10);
            var users = new List<Tuple<User, Message?, bool>>();
            foreach (var userPair in userPairs)
            {
                var lastMessage = await GetLastMessage(userPair.Item1.Id, userPair.Item2.Id);
                var isUnread = lastMessage != null && (lastMessage.Reciever == userId && !lastMessage.IsRead);
                if (userPair.Item1.Id != userId)
                    users.Add(Tuple.Create(userPair.Item1, lastMessage, isUnread));
                else
                    users.Add(Tuple.Create(userPair.Item2, lastMessage, isUnread));
            }
            return new PaginatedList<Tuple<User, Message?, bool>>(users, userPairs.Count, pageIndex, 10);
        }

        public async Task<PaginatedList<Message>> GetUserMessages(int pageIndex, Guid userId1, Guid userId2)
        {
            var query = from message in dbcontext.Messages
                        where (message.Reciever == userId1 && message.Sender == userId2)
                        || (message.Reciever == userId2 && message.Sender == userId1)
                        orderby message.SendTime descending
                        select message;
            return await PaginatedList<Message>.CreateAsync(query, pageIndex, 10);
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
