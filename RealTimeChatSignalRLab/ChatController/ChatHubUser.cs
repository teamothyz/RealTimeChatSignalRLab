using Microsoft.AspNetCore.SignalR;
using RealTimeChatSignalRLab.Models;

namespace RealTimeChatSignalRLab.ChatController
{
    public partial class ChatHub
    {
        public async Task SendMessageToUser(string recieverId, string content)
        {
            recieverId = recieverId.ToLower();
            var senderId = Context.UserIdentifier;
            if (string.IsNullOrWhiteSpace(senderId)) return;
            var sender = await _userRepository.GetUserById(Guid.Parse(senderId));
            var reciever = await _userRepository.GetUserById(Guid.Parse(recieverId));
            if (reciever == null || sender == null) return;

            var message = new Message()
            {
                Id = Guid.NewGuid(),
                Sender = Guid.Parse(senderId),
                Reciever = Guid.Parse(recieverId),
                Content = content,
                RecieverType = RecieverType.User,
                SendTime = DateTime.UtcNow.Ticks,
                IsRead = false
            };

            await _messageRepository.Send(message);
            await Clients.User(senderId).SendAsync("UserSendMessage", message, reciever);
            if (UserHandler.GetConnection(recieverId).Any())
                await Clients.User(recieverId).SendAsync("UserRecieveMessage", message, sender);
        }

        public async Task SeenUserMessage(Guid senderId)
        {
            var recieverId = Context.UserIdentifier;
            if (string.IsNullOrWhiteSpace(recieverId)) return;

            var readTask = _messageRepository.ReadMessage(senderId, Guid.Parse(recieverId));
            var sendTask = Clients.User(recieverId).SendAsync("UserSeenMessage", senderId);
            await Task.WhenAll(readTask, sendTask);
        }
    }
}
