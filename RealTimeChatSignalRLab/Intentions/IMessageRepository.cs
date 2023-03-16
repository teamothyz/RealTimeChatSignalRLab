using RealTimeChatSignalRLab.Models;
using RealTimeChatSignalRLab.Pagination;

namespace RealTimeChatSignalRLab.Intentions
{
    public interface IMessageRepository
    {
        Task<List<Tuple<User, Message>>> GetUserMessages(int pageIndex, Guid userId1, Guid userId2, long? offsetTime);
        Task<List<Tuple<User, Message>>> GetGroupMessages(int pageIndex, Guid userId, Guid groupId, long? offsetTime);
        Task Send(Message message);
        Task<List<Tuple<User, Message?, bool>>> GetUserChat(Guid userId, long? offsetTime);
        Task ReadMessage(Guid sender, Guid reciever);
    }
}
