namespace RealTimeChatSignalRLab.Models
{
    public class UserGroup
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public long LastSeen { get; set; }
    }
}
