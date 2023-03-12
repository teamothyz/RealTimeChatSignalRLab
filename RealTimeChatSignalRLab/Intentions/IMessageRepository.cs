using RealTimeChatSignalRLab.Models;
using RealTimeChatSignalRLab.Pagination;

namespace RealTimeChatSignalRLab.Intentions
{
    public interface IMessageRepository
    {
        Task<PaginatedList<Message>> GetUserMessages(int pageIndex, Guid userId1, Guid userId2);
        Task<PaginatedList<Tuple<User, Message>>> GetGroupMessages(int pageIndex, Guid userId, Guid groupId);
        Task Send(Message message);
        Task<PaginatedList<Tuple<User, Message?, bool>>> GetUserChat(int pageIndex, Guid userId);
        Task ReadMessage(Guid sender, Guid reciever);
    }
}
