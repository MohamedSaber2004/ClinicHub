namespace ClinicHub.Application.Common.Interfaces
{
    public interface IChatConnectionManager
    {
        void ConnectUser(Guid userId, string connectionId);
        void DisconnectUser(string connectionId);
        void DisconnectUser(Guid userId, string connectionId);
        IEnumerable<string> GetUserConnections(Guid userId);
        IEnumerable<Guid> GetOnlineUsers();
        
        void SetUserTyping(Guid conversationId, Guid userId, bool isTyping);
        IEnumerable<Guid> GetTypingUsers(Guid conversationId);

        void SetActiveConversation(Guid userId, Guid? conversationId);
        bool IsUserInConversation(Guid userId, Guid conversationId);
    }
}
