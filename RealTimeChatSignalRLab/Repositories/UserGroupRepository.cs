﻿using Microsoft.EntityFrameworkCore;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;
using RealTimeChatSignalRLab.Pagination;

namespace RealTimeChatSignalRLab.Repositories
{
    public class UserGroupRepository : IUserGroupRepository
    {
        private readonly ChatDBContext dbcontext;

        public UserGroupRepository(ChatDBContext context)
        {
            dbcontext = context;
        }

        public async Task AddGroupUsers(Guid groupId, Guid ownerId, List<Guid> userIds)
        {
            var owned = await dbcontext.Groups.AnyAsync(g => g.Id == groupId && g.Owner == ownerId);
            if (!owned) throw new Exception("User is not the owner of this group");

            foreach (var id in userIds)
            {
                var existed = await dbcontext.UserGroups.AnyAsync(g => g.GroupId == groupId
                    && g.UserId == id);
                if (existed) continue;
                var ug = new UserGroup
                {
                    UserId = id,
                    GroupId = groupId,
                    Id = Guid.NewGuid()
                };
                await dbcontext.UserGroups.AddAsync(ug);
            }
            await dbcontext.SaveChangesAsync();
        }

        public async Task DeleteGroupUsers(Guid groupId, Guid ownerId, List<Guid> userIds)
        {
            var owned = await dbcontext.Groups.AnyAsync(g => g.Id == groupId && g.Owner == ownerId);
            if (!owned) throw new Exception("User is not the owner of this group");

            var userGroups = await dbcontext.UserGroups
                .Where(g => g.GroupId == groupId && userIds.Contains(g.UserId))
                .ToListAsync();
            dbcontext.UserGroups.RemoveRange(userGroups);
            await dbcontext.SaveChangesAsync();
        }

        public async Task<bool> CheckJoining(Guid userId, Guid groupId)
        {
            return await dbcontext.UserGroups.AnyAsync(g => g.UserId == userId && g.GroupId == groupId);
        }

        public async Task<PaginatedList<Tuple<Group, Message, bool>>> GetGroupsByUserId(int pageIndex, Guid userId)
        {
            var query = from grUser in dbcontext.UserGroups
                        join gr in dbcontext.Groups on grUser.GroupId equals gr.Id

                        join message in dbcontext.Messages on
                        new { id = gr.Id, type = RecieverType.Group }
                        equals new { id = message.Reciever, type = message.RecieverType }
                        into messageJoin
                        from message in messageJoin.DefaultIfEmpty()

                        where grUser.UserId == userId
                        group new { gr, message } by gr.Id into g

                        select g.OrderByDescending(x => (x.message != null ? x.message.SendTime : 0))
                        .Select(x => Tuple.Create(x.gr, x.message)).First();
            var groupAndLastMessages = await query.ToListAsync();
            var groups = groupAndLastMessages.OrderByDescending(x => x.Item2?.SendTime ?? 0)
                .Skip((pageIndex - 1) * 10).Take(10);
            var groupsWithUnread = new List<Tuple<Group, Message, bool>>();
            foreach (var group in groups)
            {
                var isUnread = await CheckUnreadGroupMessage(userId, group.Item1.Id);
                groupsWithUnread.Add(Tuple.Create(group.Item1, group.Item2, isUnread));
            }
            return new PaginatedList<Tuple<Group, Message, bool>>(groupsWithUnread, groupAndLastMessages.Count, pageIndex, 10);
        }

        public async Task<List<Group>> GetAllGroupsByUserId(Guid userId)
        {
            var query = (from grUser in dbcontext.UserGroups
                        join gr in dbcontext.Groups on grUser.GroupId equals gr.Id
                        where grUser.UserId == userId
                        select gr).Distinct();
            return await query.ToListAsync();
        }

        public async Task<bool> CheckUnreadGroupMessage(Guid userId, Guid groupId)
        {
            var joined = await CheckJoining(userId, groupId);
            if (!joined) throw new Exception("User does not join this group");

            var lastSeenTime = (await dbcontext.UserGroups
                .FirstAsync(g => g.UserId == userId && g.GroupId == groupId)).LastSeen;
            var query = from message in dbcontext.Messages
                        where message.Reciever == groupId
                        && message.RecieverType == RecieverType.Group
                        orderby message.SendTime descending
                        select message.SendTime;
            var lastRecieveTime = await query.FirstOrDefaultAsync();
            return lastSeenTime < lastRecieveTime;
        }

        public async Task<PaginatedList<User>> GetGroupUsersByGroupId(int pageIndex, Guid userId, Guid groupId)
        {
            var joined = await CheckJoining(userId, groupId);
            if (!joined) throw new Exception("User does not join this group");

            var query = (from grUser in dbcontext.UserGroups
                         join user in dbcontext.Users on grUser.UserId equals user.Id
                         where grUser.GroupId == groupId
                         select user).Distinct();
            return await PaginatedList<User>.CreateAsync(query, pageIndex, 10);
        }

        public async Task UpdateReadMessageTime(Guid userId, Guid groupId)
        {
            var joined = await CheckJoining(userId, groupId);
            if (!joined) throw new Exception("User does not join this group");

            var userGroup = await dbcontext.UserGroups.SingleAsync(g => g.UserId == userId && g.GroupId == groupId);
            userGroup.LastSeen = DateTime.UtcNow.Ticks;
            dbcontext.UserGroups.Update(userGroup);
            await dbcontext.SaveChangesAsync();
        }
    }
}