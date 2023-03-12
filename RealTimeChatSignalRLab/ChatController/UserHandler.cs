namespace RealTimeChatSignalRLab.ChatController
{
    public static class UserHandler
    {
        private static readonly Dictionary<string, List<string>> _connections = new Dictionary<string, List<string>>();

        private static Dictionary<string, List<string>> Connections
        {
            get { return _connections; }
        }

        public static void AddNewConnection(string userId, string connectionId)
        {
            if (Connections.ContainsKey(userId))
            {
                Connections[userId].Add(connectionId);
                return;
            }
            Connections.Add(userId, new List<string>() { connectionId });
        }

        public static void RemoveConnection(string userId, string connectionId)
        {
            if (Connections.ContainsKey(userId))
            {
                Connections[userId].Remove(connectionId);
            }
        }

        public static List<string> GetConnection(string userId)
        {
            if (Connections.ContainsKey(userId.ToLower()))
            {
                return Connections[userId.ToLower()];
            }
            return new List<string>();
        }
    }
}
