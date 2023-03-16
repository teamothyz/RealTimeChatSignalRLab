using RealTimeChatSignalRLab.Models;

namespace RealTimeChatSignalRLab.Intentions
{
    public interface IUserGroupRepository
    {
        Task AddGroupUsers(Guid groupId, Guid ownerId, List<Guid> userIds);
        Task DeleteGroupUsers(Guid groupId, Guid ownerId, List<Guid> userIds);
        Task<List<Group>> GetAllGroupsByUserId(Guid userId);
        Task<bool> CheckJoining(Guid userId, Guid groupId);
        Task<bool> CheckUnreadGroupMessage(Guid userId, Guid groupId);
        Task<List<User>> GetGroupUsersByGroupId(int pageIndex, Guid userId, Guid groupId);
        Task<List<Tuple<Group, Message?, bool>>> GetGroupsByUserId(Guid userId, long? offsetTime);
        Task UpdateReadMessageTime(Guid userId, Guid groupId);
    }
}