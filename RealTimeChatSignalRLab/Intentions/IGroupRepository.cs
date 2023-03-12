using RealTimeChatSignalRLab.Models;

namespace RealTimeChatSignalRLab.Intentions
{
    public interface IGroupRepository
    {
        Task<Group> Create(Group group);
        Task<Group> GetGroup(Guid groupId);
        Task<bool> CheckOwner(Guid userId, Guid groupId);
        Task DeleteGroup(Guid userId, Guid groupId);
    }
}
