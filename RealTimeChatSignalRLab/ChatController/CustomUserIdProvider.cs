using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace RealTimeChatSignalRLab.ChatController
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            try
            {
                return connection.GetHttpContext()?
                    .User.Claims?
                    .First(claim => claim.Type == ClaimTypes.NameIdentifier)?
                    .Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
