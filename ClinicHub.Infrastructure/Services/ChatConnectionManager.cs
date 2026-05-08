using System.Collections.Concurrent;
using ClinicHub.Application.Common.Interfaces;

namespace ClinicHub.Infrastructure.Services
{
    public class ChatConnectionManager : IChatConnectionManager
    {
        // UserId -> HashSet of ConnectionIds
        private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
        
        // ConversationId -> HashSet of UserIds currently typing
        private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _typingUsers = new();

        // UserId -> Active ConversationId
        private readonly ConcurrentDictionary<Guid, Guid> _activeConversations = new();

        public void ConnectUser(Guid userId, string connectionId)
        {
            _userConnections.AddOrUpdate(
                userId,
                new HashSet<string> { connectionId },
                (_, connections) =>
                {
                    lock (connections)
                    {
                        connections.Add(connectionId);
                    }
                    return connections;
                });
        }

        public void DisconnectUser(string connectionId)
        {
            // This is O(N) where N is number of online users. 
            // Acceptable for in-memory if N isn't massive.
            foreach (var kvp in _userConnections)
            {
                lock (kvp.Value)
                {
                    if (kvp.Value.Remove(connectionId))
                    {
                        if (kvp.Value.Count == 0)
                        {
                            _userConnections.TryRemove(kvp.Key, out _);
                            _activeConversations.TryRemove(kvp.Key, out _);
                            // Also clear from typing users
                            foreach (var typers in _typingUsers.Values)
                            {
                                lock (typers) { typers.Remove(kvp.Key); }
                            }
                        }
                        break;
                    }
                }
            }
        }

        public void DisconnectUser(Guid userId, string connectionId)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                        _activeConversations.TryRemove(userId, out _);
                        // Also clear from typing users
                        foreach (var typers in _typingUsers.Values)
                        {
                            lock (typers) { typers.Remove(userId); }
                        }
                    }
                }
            }
        }

        public IEnumerable<string> GetUserConnections(Guid userId)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    return connections.ToList();
                }
            }
            return Enumerable.Empty<string>();
        }

        public IEnumerable<Guid> GetOnlineUsers()
        {
            return _userConnections.Keys.ToList();
        }

        public void SetUserTyping(Guid conversationId, Guid userId, bool isTyping)
        {
            if (isTyping)
            {
                _typingUsers.AddOrUpdate(
                    conversationId,
                    new HashSet<Guid> { userId },
                    (_, typers) =>
                    {
                        lock (typers)
                        {
                            typers.Add(userId);
                        }
                        return typers;
                    });
            }
            else
            {
                if (_typingUsers.TryGetValue(conversationId, out var typers))
                {
                    lock (typers)
                    {
                        typers.Remove(userId);
                        if (typers.Count == 0)
                        {
                            _typingUsers.TryRemove(conversationId, out _);
                        }
                    }
                }
            }
        }

        public IEnumerable<Guid> GetTypingUsers(Guid conversationId)
        {
            if (_typingUsers.TryGetValue(conversationId, out var typers))
            {
                lock (typers)
                {
                    return typers.ToList();
                }
            }
            return Enumerable.Empty<Guid>();
        }

        public void SetActiveConversation(Guid userId, Guid? conversationId)
        {
            if (conversationId.HasValue)
            {
                _activeConversations.AddOrUpdate(userId, conversationId.Value, (_, _) => conversationId.Value);
            }
            else
            {
                _activeConversations.TryRemove(userId, out _);
            }
        }

        public bool IsUserInConversation(Guid userId, Guid conversationId)
        {
            return _activeConversations.TryGetValue(userId, out var activeConv) && activeConv == conversationId;
        }
    }
}
