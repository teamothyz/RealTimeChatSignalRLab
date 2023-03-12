using RealTimeChatSignalRLab.Models;
using RealTimeChatSignalRLab.Pagination;

namespace RealTimeChatSignalRLab.Intentions
{
    public interface IUserGroupRepository
    {
        Task AddGroupUsers(Guid groupId, Guid ownerId, List<Guid> userIds);
        Task DeleteGroupUsers(Guid groupId, Guid ownerId, List<Guid> userIds);
        Task<List<Group>> GetAllGroupsByUserId(Guid userId);
        Task<bool> CheckJoining(Guid userId, Guid groupId);
        Task<bool> CheckUnreadGroupMessage(Guid userId, Guid groupId);
        Task<PaginatedList<User>> GetGroupUsersByGroupId(int pageIndex, Guid userId, Guid groupId);
        Task<PaginatedList<Tuple<Group, Message, bool>>> GetGroupsByUserId(int pageIndex, Guid userId);
        Task UpdateReadMessageTime(Guid userId, Guid groupId);
    }
}