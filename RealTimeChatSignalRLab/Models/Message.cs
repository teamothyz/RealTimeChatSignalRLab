namespace RealTimeChatSignalRLab.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid Sender { get; set; }
        public Guid Reciever { get; set; }
        public string Content { get; set; }
        public RecieverType RecieverType { get; set; }
        public long SendTime { get; set; }
        public bool IsRead { get; set; }
    }

    public enum RecieverType
    {
        User, Group
    }
}
