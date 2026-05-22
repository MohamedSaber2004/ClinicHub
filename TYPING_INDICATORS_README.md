# ⌨️ Typing Indicators in Conversations Guide

**Version:** 1.0  
**Last Updated:** May 2026  
**Language:** العربية و English  
**Technology:** SignalR / Pusher Real-time

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Setting Typing Status](#setting-typing-status)
4. [Getting Typing Status](#getting-typing-status)
5. [Real-Time Event Flow](#real-time-event-flow)
6. [Implementation Guide](#implementation-guide)
7. [Pusher Events Reference](#pusher-events-reference)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

---

## 🎯 Overview

Typing indicators in ClinicHub provide real-time feedback showing when other users are composing messages in a conversation. This feature enhances user experience by indicating active typing activity without waiting for the complete message.

### Key Features:
- ✅ Real-time typing status updates across all conversation participants
- ✅ Automatic timeout (removes typing indicator after inactivity)
- ✅ Per-user typing state in conversation
- ✅ Conversation-list awareness (optional: show typing status in list)
- ✅ Broadcast to all conversation members
- ✅ Bi-directional: Set status (send) and Get status (receive)

---

## 🏗️ Architecture

### Typing Indicator Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                    TYPING INDICATOR LIFECYCLE                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  User Opens Message Input                                       │
│         ↓                                                        │
│  [SET] User starts typing → Send "typing.start" event           │
│         ↓                                                        │
│  [GET] Other users receive event via Pusher channel            │
│         ↓                                                        │
│  UI Updates: Show "[User Name] is typing..."                   │
│         ↓                                                        │
│  User continues/stops typing                                   │
│         ↓                                                        │
│  [SET] Send "typing.end" event OR auto-timeout after 5 sec    │
│         ↓                                                        │
│  UI Updates: Hide typing indicator                             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### System Actors

| Actor | Role |
|-------|------|
| **Sender** | User currently typing (SET typing status) |
| **Recipients** | Other conversation members (GET typing status) |
| **Server** | Validates and broadcasts typing events |
| **Pusher/SignalR** | Real-time message delivery |
| **Database** (Optional) | Audit trail (not required for real-time display) |

---

## 🔧 Setting Typing Status

### 📤 API Endpoint: SET Typing Status

#### Endpoint Definition
```
POST /api/v1/conversations/{conversationId}/typing
```

#### Request Body
```json
{
  "isTyping": true,
  "timestamp": "2026-05-22T10:30:45.123Z"
}
```

#### Request Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `conversationId` | UUID | Yes | ID of the conversation |
| `isTyping` | bool | Yes | `true` = start typing, `false` = stop typing |
| `timestamp` | datetime | No | Client-side timestamp for clock-sync |

#### Response: Success (200 OK)
```json
{
  "succeeded": true,
  "message": "Typing status updated",
  "data": {
    "conversationId": "550e8400-e29b-41d4-a716-446655440000",
    "userId": "660e8400-e29b-41d4-a716-446655440001",
    "isTyping": true,
    "broadcastedAt": "2026-05-22T10:30:45.789Z"
  }
}
```

#### Response: Error (400 Bad Request)
```json
{
  "succeeded": false,
  "message": "Invalid conversation ID",
  "errors": ["ConversationNotFound"]
}
```

#### Implementation (Backend - Application Layer)

**Command Class:**
```csharp
// ClinicHub.Application/Features/Conversations/Commands/SetTypingStatusCommand.cs
public class SetTypingStatusCommand : IRequest<Result<SetTypingStatusDto>>
{
    public Guid ConversationId { get; set; }
    public bool IsTyping { get; set; }
    public DateTime? Timestamp { get; set; }
}
```

**Command Handler:**
```csharp
// ClinicHub.Application/Features/Conversations/Commands/SetTypingStatusCommandHandler.cs
public class SetTypingStatusCommandHandler : IRequestHandler<SetTypingStatusCommand, Result<SetTypingStatusDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITypingIndicatorService _typingService;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public async Task<Result<SetTypingStatusDto>> Handle(SetTypingStatusCommand request, CancellationToken cancellationToken)
    {
        // Validate conversation exists and user is member
        var conversation = await _unitOfWork.ConversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation == null)
            return Result<SetTypingStatusDto>.Failure("Conversation not found");

        var userId = _currentUser.UserId;
        var isMember = await _unitOfWork.ConversationRepository
            .IsUserMemberAsync(request.ConversationId, userId);
        
        if (!isMember)
            return Result<SetTypingStatusDto>.Failure("You are not a member of this conversation");

        // Broadcast typing event to other members
        var broadcastedAt = request.Timestamp ?? DateTime.UtcNow;
        await _typingService.BroadcastTypingStatusAsync(
            conversationId: request.ConversationId,
            userId: userId,
            isTyping: request.IsTyping,
            broadcastedAt: broadcastedAt
        );

        return Result<SetTypingStatusDto>.Success(new SetTypingStatusDto
        {
            ConversationId = request.ConversationId,
            UserId = userId,
            IsTyping = request.IsTyping,
            BroadcastedAt = broadcastedAt
        });
    }
}
```

**Controller Action:**
```csharp
// ClinicHub.API/Controllers/Version1/ConversationsController.cs
[HttpPost("{conversationId}/typing")]
public async Task<IActionResult> SetTypingStatus(
    Guid conversationId, 
    [FromBody] SetTypingStatusDto dto)
{
    var command = new SetTypingStatusCommand
    {
        ConversationId = conversationId,
        IsTyping = dto.IsTyping,
        Timestamp = dto.Timestamp
    };
    
    var result = await Mediator.Send(command);
    return result.Succeeded ? Ok(result) : BadRequest(result);
}
```

---

## 📥 Getting Typing Status

### 🔍 Real-Time Listening (Pusher Channels)

Typing status is **NOT** retrieved via HTTP GET. Instead, clients **listen** to real-time events via Pusher/SignalR channels.

#### Channel Subscription
```
Channel: private-conversation-{conversationId}
Event: typing.status
```

#### Event Structure (Pushed from Server)
```json
{
  "event": "typing.status",
  "channel": "private-conversation-550e8400-e29b-41d4-a716-446655440000",
  "data": {
    "conversationId": "550e8400-e29b-41d4-a716-446655440000",
    "userId": "660e8400-e29b-41d4-a716-446655440001",
    "userName": "Dr. Ahmed Hassan",
    "isTyping": true,
    "typingStartedAt": "2026-05-22T10:30:45.123Z"
  }
}
```

#### Event Structure When Typing Stops
```json
{
  "event": "typing.status",
  "channel": "private-conversation-550e8400-e29b-41d4-a716-446655440000",
  "data": {
    "conversationId": "550e8400-e29b-41d4-a716-446655440000",
    "userId": "660e8400-e29b-41d4-a716-446655440001",
    "userName": "Dr. Ahmed Hassan",
    "isTyping": false,
    "typingEndedAt": "2026-05-22T10:30:50.456Z"
  }
}
```

#### Implementation (Backend - Real-Time Service)

**Typing Service Interface:**
```csharp
// ClinicHub.Application/Services/ITypingIndicatorService.cs
public interface ITypingIndicatorService
{
    Task BroadcastTypingStatusAsync(
        Guid conversationId,
        Guid userId,
        bool isTyping,
        DateTime broadcastedAt);
    
    Task<IEnumerable<TypingUser>> GetActiveTypersAsync(Guid conversationId);
}
```

**Typing Service Implementation (Pusher):**
```csharp
// ClinicHub.Infrastructure/Services/PusherTypingIndicatorService.cs
public class PusherTypingIndicatorService : ITypingIndicatorService
{
    private readonly IPusherClient _pusher;
    private readonly IUnitOfWork _unitOfWork;

    public async Task BroadcastTypingStatusAsync(
        Guid conversationId,
        Guid userId,
        bool isTyping,
        DateTime broadcastedAt)
    {
        var channelName = $"private-conversation-{conversationId}";
        
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        
        var @event = new
        {
            conversationId = conversationId,
            userId = userId,
            userName = user?.FullName ?? "Unknown",
            isTyping = isTyping,
            typingStartedAt = isTyping ? broadcastedAt : (DateTime?)null,
            typingEndedAt = !isTyping ? broadcastedAt : (DateTime?)null
        };

        // Broadcast to Pusher
        await _pusher.TriggerAsync(
            channels: new[] { channelName },
            eventName: "typing.status",
            data: @event,
            excludeSocketId: null // Send to all members including sender
        );
    }

    public async Task<IEnumerable<TypingUser>> GetActiveTypersAsync(Guid conversationId)
    {
        // Optional: Track active typers in Redis cache
        // Used for conversation list display or diagnostics
        var redisKey = $"typing:{conversationId}";
        var activeTypers = await _redis.GetAsync<List<TypingUser>>(redisKey);
        return activeTypers ?? new List<TypingUser>();
    }
}
```

---

## 🔄 Real-Time Event Flow

### Complete Flow Example

```
┌─ SENDER SIDE ──────────────────────────────────────────────────┐
│                                                                 │
│  1. User starts typing in message input                        │
│     └─→ Call: POST /api/v1/conversations/{id}/typing           │
│         Body: { "isTyping": true }                             │
│                                                                 │
│  2. Server validates and broadcasts event                      │
│     └─→ Pusher receives: trigger typing.status event           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

                            ↓ (via Pusher)

┌─ RECIPIENTS SIDE ──────────────────────────────────────────────┐
│                                                                 │
│  3. Other members receive: typing.status event                 │
│     Channel: private-conversation-{conversationId}             │
│     Event data:                                                │
│     {                                                          │
│       "userId": "...",                                         │
│       "userName": "Dr. Ahmed",                                 │
│       "isTyping": true,                                        │
│       "typingStartedAt": "2026-05-22T10:30:45.123Z"           │
│     }                                                          │
│                                                                 │
│  4. UI Updates                                                 │
│     └─→ Show "Dr. Ahmed is typing..." indicator               │
│                                                                 │
│  5. Auto-remove after timeout (5 seconds)                      │
│     └─→ If no new typing event received, hide indicator       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

                    ↓ (User stops typing or sends message)

┌─ SENDER SIDE (Cleanup) ────────────────────────────────────────┐
│                                                                 │
│  6. User sends message or stops typing                         │
│     └─→ Call: POST /api/v1/conversations/{id}/typing           │
│         Body: { "isTyping": false }                            │
│                                                                 │
│  7. Server broadcasts stop event                               │
│     └─→ Pusher trigger: typing.status with isTyping: false    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Sequence Diagram

```
Client A              Server              Pusher              Client B
  │                    │                   │                    │
  ├─ POST /typing ────→ │                   │                    │
  │  (isTyping:true)    │                   │                    │
  │                    ├─ Validate auth    │                    │
  │                    ├─ Trigger event ──→ │                    │
  │                    │                   ├─ Push to subs ────→ │
  │ ◄─ 200 OK ─────────┤                   │              Update UI
  │                    │                   │ (show typing...)    │
  │                    │                   │                    │
  │ ... typing ...     │                   │ ... 5 sec timeout   │
  │                    │                   │                    │
  ├─ POST /typing ────→ │                   │                    │
  │  (isTyping:false)   │                   │                    │
  │                    ├─ Trigger event ──→ │                    │
  │                    │                   ├─ Push to subs ────→ │
  │ ◄─ 200 OK ─────────┤                   │              Update UI
  │                    │                   │ (hide typing...)    │
  │                    │                   │                    │
```

---

## 📚 Implementation Guide

### Step 1: Create DTOs

```csharp
// ClinicHub.Application/Features/Conversations/DTOs/TypingStatusDto.cs
public class TypingStatusDto
{
    public bool IsTyping { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class SetTypingStatusDto
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public bool IsTyping { get; set; }
    public DateTime BroadcastedAt { get; set; }
}

public class TypingUser
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public DateTime TypingStartedAt { get; set; }
}
```

### Step 2: Create Service Interface & Implementation

```csharp
// Already shown above: ITypingIndicatorService and PusherTypingIndicatorService
```

### Step 3: Register in Dependency Injection

```csharp
// ClinicHub.Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // ... existing code ...
    
    services.AddScoped<ITypingIndicatorService, PusherTypingIndicatorService>();
    
    return services;
}
```

### Step 4: Add Route

```csharp
// ClinicHub.API/Routes/ApiRoutes.cs
public static class ApiRoutes
{
    public static class Conversations
    {
        private const string Base = $"{Version}/conversations";
        
        public const string SetTypingStatus = $"{Base}/{{conversationId}}/typing";
        public const string GetConversations = Base;
        // ... other routes ...
    }
}
```

### Step 5: Add Controller Action

```csharp
// Already shown above in SetTypingStatus section
```

---

## 📡 Pusher Events Reference

### Event: typing.status

**When Triggered:**
- User starts typing (`isTyping: true`)
- User stops typing (`isTyping: false`)
- User sends message (implicit `isTyping: false`)

**Channel:** `private-conversation-{conversationId}`

**Event Payload:**
```json
{
  "conversationId": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "660e8400-e29b-41d4-a716-446655440001",
  "userName": "Dr. Ahmed Hassan",
  "isTyping": true,
  "typingStartedAt": "2026-05-22T10:30:45.123Z",
  "typingEndedAt": null
}
```

### Related Events

| Event | Channel | Purpose |
|-------|---------|---------|
| `typing.status` | `private-conversation-{id}` | Broadcast typing indicator |
| `message.new` | `private-conversation-{id}` | New message (clears typing) |
| `conversation.member-joined` | `presence-conversations` | Member joined (clear old typing) |
| `conversation.member-left` | `presence-conversations` | Member left (clear typing) |

---

## ✅ Best Practices

### 1. **Throttle Typing Events** (Client-Side)
Send typing start event only once, don't spam repeatedly.

```csharp
private DateTime _lastTypingNotification = DateTime.MinValue;
private const int TYPING_THROTTLE_MS = 3000; // Send max every 3 seconds

public async Task OnUserTyping()
{
    var now = DateTime.UtcNow;
    if ((now - _lastTypingNotification).TotalMilliseconds > TYPING_THROTTLE_MS)
    {
        await SetTypingStatusAsync(isTyping: true);
        _lastTypingNotification = now;
    }
}
```

### 2. **Auto-Timeout** (Client & Server)
Automatically clear typing indicator if no update received within 5 seconds.

```csharp
private Timer _typingTimeoutTimer;
private const int TYPING_TIMEOUT_MS = 5000; // 5 seconds

public void OnTypingEventReceived()
{
    _typingTimeoutTimer?.Dispose();
    _typingTimeoutTimer = new Timer(_ => 
    {
        // Clear typing indicator from UI
        _typingIndicators.Remove(userId);
        UI.UpdateTypingList(_typingIndicators);
    }, null, TYPING_TIMEOUT_MS, Timeout.Infinite);
}
```

### 3. **Clean Up on Message Send**
Always send `isTyping: false` when message is sent.

```csharp
public async Task SendMessage(string content)
{
    // Send message
    var message = await SendMessageAsync(content);
    
    // Clear typing indicator
    await SetTypingStatusAsync(isTyping: false);
    
    // UI update
    UI.ClearTypingIndicator();
}
```

### 4. **Handle Disconnections**
Clear all typing indicators when user disconnects.

```csharp
public async Task OnConnectionLost()
{
    // Broadcast immediate stop to all conversations
    foreach (var convId in _activeConversations)
    {
        await SetTypingStatusAsync(convId, isTyping: false);
    }
    
    _typingIndicators.Clear();
}
```

### 5. **Exclude Sender from Event**
Consider: Should sender see their own typing indicator?
- **No**: Use `excludeSocketId` in Pusher to exclude sender
- **Yes**: Send to all members including sender

---

## 🔧 Troubleshooting

### Issue 1: Typing Indicator Stuck (Not Clearing)

**Symptoms:**
- UI shows "User is typing..." but user already sent message
- Indicator doesn't clear after 5 seconds

**Solutions:**
1. ✅ Ensure `isTyping: false` is sent on message send
2. ✅ Verify auto-timeout is implemented (5 sec max)
3. ✅ Check Pusher subscription to `private-conversation-{id}` channel
4. ✅ Verify user ID in events matches current user

### Issue 2: Typing Event Not Received

**Symptoms:**
- Sender sends typing event, recipients don't see indicator
- No error in API response

**Solutions:**
1. ✅ Verify Pusher API key is correct
2. ✅ Check user is authenticated (`isTyping: false`)
3. ✅ Verify recipient is subscribed to conversation channel
4. ✅ Check Pusher dashboard for event delivery
5. ✅ Verify channel name format: `private-conversation-{guid}`

### Issue 3: Too Many API Calls (Throttling)

**Symptoms:**
- Client sending typing event on every keystroke
- Server logs show excessive calls
- Rate limit errors

**Solutions:**
1. ✅ Implement client-side throttling (3-5 sec intervals)
2. ✅ Send `isTyping: false` only once, not repeatedly
3. ✅ Use timer-based approach instead of event-based

### Issue 4: Authorization Errors

**Symptoms:**
- API returns 401 Unauthorized for typing endpoint
- Pusher channel subscription fails

**Solutions:**
1. ✅ Verify user is authenticated (check token)
2. ✅ Verify user is member of conversation
3. ✅ Check Pusher auth endpoint is working
4. ✅ Verify `ICurrentUserService` returns correct user ID

---

## 📝 Additional Resources

- [MESSAGE_STATES_README.md](MESSAGE_STATES_README.md) - Message delivery states
- [REALTIME_CONNECT_DISCONNECT_FLOW.md](REALTIME_CONNECT_DISCONNECT_FLOW.md) - Real-time connection lifecycle
- [REALTIME_FLUTTER_GUIDE.md](REALTIME_FLUTTER_GUIDE.md) - Flutter implementation
- Pusher Documentation: https://pusher.com/docs
- SignalR Documentation: https://learn.microsoft.com/en-us/aspnet/core/signalr/

---

**Questions or Issues?** Please contact the development team or create an issue in the repository.
