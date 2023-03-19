using Microsoft.AspNetCore.SignalR;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;

namespace RealTimeChatSignalRLab.ChatController
{
    public partial class ChatHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IUserGroupRepository _userGroupRepository;

        public ChatHub(IMessageRepository messageRepository,
            IGroupRepository groupRepository,
            IUserGroupRepository userGroupRepository,
            IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _userGroupRepository = userGroupRepository;
        }

        public override async Task<Task> OnConnectedAsync()
        {
            if (Context.UserIdentifier != null)
            {
                UserHandler.AddNewConnection(Context.UserIdentifier, Context.ConnectionId);
                await InitGroups();
                await OnlineNotify();
            }
            return base.OnConnectedAsync();
        }

        public override async Task<Task> OnDisconnectedAsync(Exception? exception)
        {
            if (Context.UserIdentifier != null)
            {
                UserHandler.RemoveConnection(Context.UserIdentifier, Context.ConnectionId);
                await OfflineNotify();
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}