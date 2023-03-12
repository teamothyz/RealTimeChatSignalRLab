using Microsoft.EntityFrameworkCore;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;

namespace RealTimeChatSignalRLab.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ChatDBContext dbcontext;

        public GroupRepository(ChatDBContext context)
        {
            dbcontext = context;
        }

        public async Task<Group> Create(Group group)
        {
            await dbcontext.Groups.AddAsync(group);
            var usergroup = new UserGroup
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                UserId = group.Owner
            };
            await dbcontext.UserGroups.AddAsync(usergroup);
            dbcontext.SaveChanges();
            return group;
        }

        public async Task DeleteGroup(Guid userId, Guid groupId)
        {
            var owned = await CheckOwner(userId, groupId);
            if (!owned) throw new Exception("User is not the owner of this group");

            var group = await dbcontext.Groups.FirstAsync(group => group.Id == groupId);
            dbcontext.Groups.Remove(group);
            dbcontext.SaveChanges();
        }

        public async Task<bool> CheckOwner(Guid userId, Guid groupId)
        {
            return await dbcontext.Groups.AnyAsync(g => g.Owner == userId && g.Id == groupId);
        }

        public async Task<Group> GetGroup(Guid groupId)
        {
            return await dbcontext.Groups.FirstAsync(g => g.Id == groupId);
        }
    }
}
