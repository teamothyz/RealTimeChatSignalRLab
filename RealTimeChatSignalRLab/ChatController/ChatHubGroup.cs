using Microsoft.AspNetCore.SignalR;
using RealTimeChatSignalRLab.Models;
using System.Reflection;

namespace RealTimeChatSignalRLab.ChatController
{
    public partial class ChatHub
    {
        private async Task InitGroups()
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrWhiteSpace(userId)) return;
            var groups = await _userGroupRepository.GetAllGroupsByUserId(Guid.Parse(userId));
            groups.ForEach(async group => { await AddToGroup(userId, group.Id); });
        }

        public async Task SendMessageToGroup(Guid groupId, string message)
        {
            var senderId = Context.UserIdentifier;
            var username = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(senderId)) return;
            var joined = await _userGroupRepository.CheckJoining(Guid.Parse(senderId), groupId);
            if (!joined) throw new Exception("Not joined");

            var group = await _groupRepository.GetGroup(groupId);
            var mess = new Message()
            {
                Id = Guid.NewGuid(),
                Sender = Guid.Parse(senderId),
                Reciever = group.Id,
                Content = message,
                RecieverType = RecieverType.Group,
                SendTime = DateTime.UtcNow.Ticks,
                IsRead = false
            };
            var addToDTBTask = _messageRepository.Send(mess);
            var sendTask = Clients.Group(groupId.ToString()).SendAsync("GroupMessage", mess, group, username);
            await Task.WhenAll(addToDTBTask, sendTask);
        }

        public async Task SeenGroupMessage(Guid groupId)
        {
            var senderId = Context.UserIdentifier;
            if (string.IsNullOrWhiteSpace(senderId)) return;
            var updateTask = _userGroupRepository.UpdateReadMessageTime(Guid.Parse(senderId), groupId);
            var seenTask = Clients.User(senderId).SendAsync("GroupMessageSeen", groupId);
            await Task.WhenAll(updateTask, seenTask);
        }

        public async Task AddMemberToGroup(string emails, Guid groupId)
        {
            var ownerId = Context.UserIdentifier;
            if (ownerId == null) return;

            var tasks = new List<Task>();
            var emailList = emails.Split(';').ToList();
            var users = await _userRepository.GetUserByEmails(emailList);

            await _userGroupRepository.AddGroupUsers(groupId, Guid.Parse(ownerId), users.Select(user => user.Id).ToList());
            users.ForEach(user => tasks.Add(AddToGroup(user.Id.ToString(), groupId)));
            await Task.WhenAll(tasks);
            var group = await _groupRepository.GetGroup(groupId);
            var message = await _messageRepository.GetLastMessage(groupId);
            await Clients.Users(users.Select(user => user.Id.ToString())).SendAsync("GroupMessage", message, group, null);
        }

        public async Task RemoveMemberFromGroup(Guid userId, Guid groupId)
        {
            var ownerId = Context.UserIdentifier;
            if (ownerId == null) return;
            await _userGroupRepository.DeleteGroupUsers(groupId, Guid.Parse(ownerId), new List<Guid> { userId }, false);
            await RemoveFromGroup(userId, groupId);
            await Clients.User(userId.ToString()).SendAsync("RemoveGroupUser", "success", groupId);
            await Clients.Group(groupId.ToString()).SendAsync("AMemberWasRemoved", userId, groupId);
        }

        public async Task LeaveGroup(Guid groupId)
        {
            var userIdString = Context.UserIdentifier;
            if (userIdString == null) return;
            var userId = Guid.Parse(userIdString);
            await _userGroupRepository.DeleteGroupUsers(groupId, userId, new List<Guid> { userId }, true);
            await RemoveFromGroup(userId, groupId);
            await Clients.User(userId.ToString()).SendAsync("RemoveGroupUser", "success", groupId);
            await Clients.Group(groupId.ToString()).SendAsync("AMemberWasRemoved", userId, groupId);
        }

        public async Task DeleteGroup(Guid groupId)
        {
            var ownerId = Context.UserIdentifier;
            if (ownerId == null) return;
            var groupUsers = await _userGroupRepository.GetGroupUsersByGroupId(null, Guid.Parse(ownerId), groupId);
            var uIDs = groupUsers.Select(user => user.Id).ToList();
            await _userGroupRepository.DeleteGroupUsers(groupId, Guid.Parse(ownerId), uIDs, false);
            await _groupRepository.DeleteGroup(Guid.Parse(ownerId), groupId);
            uIDs.ForEach(async (id) =>
            {
                await RemoveFromGroup(id, groupId);
                await Clients.User(id.ToString()).SendAsync("RemoveGroupUser", "success", groupId);
                await Clients.Group(groupId.ToString()).SendAsync("AMemberWasRemoved", id, groupId);
            });
        }

        private async Task AddToGroup(string userId, Guid groupId)
        {
            var tasks = new List<Task>();
            var connections = UserHandler.GetConnection(userId);
            connections.ForEach(connectionId => { tasks.Add(Groups.AddToGroupAsync(connectionId, groupId.ToString())); });
            await Task.WhenAll(tasks);
        }

        private async Task RemoveFromGroup(Guid userId, Guid groupId)
        {
            var tasks = new List<Task>();
            var connections = UserHandler.GetConnection(userId.ToString());
            connections.ForEach(connectionId => { tasks.Add(Groups.RemoveFromGroupAsync(connectionId, groupId.ToString())); });
            await Task.WhenAll(tasks);
        }
    }
}
