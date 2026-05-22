# Real-Time Messaging States & Scenarios

**Version:** 1.0  
**Last Updated:** May 2026  
**Language:** العربية و English

---

## 📌 Table of Contents

1. [Overview](#overview)
2. [Message States](#message-states)
3. [Delivery Scenarios](#delivery-scenarios)
4. [State Transitions](#state-transitions)
5. [Real-Time Event Handling](#real-time-event-handling)
6. [Implementation Guide](#implementation-guide)
7. [Troubleshooting](#troubleshooting)

---

## 🎯 Overview

The ClinicHub messaging system handles real-time message delivery with three primary states:

| State | Definition | Status Code |
|-------|-----------|------------|
| **Sent** | Message created and sent by sender | `Sent` (0) |
| **Delivered** | Message received by server and visible to recipient | `Delivered` (1) |
| **Read** | Message opened and read by recipient | `Read` (2) |

---

## 📊 Message States

### 1️⃣ **SENT** (Status = 0)
**Definition:** Message has been successfully transmitted from the sender's device to the server.

**Characteristics:**
- ✅ Message stored in database
- ✅ Message visible in sender's conversation
- ❌ Not yet visible to recipient (if offline)
- ❌ No read receipt received
- 🕐 Timestamp recorded on server

**When it occurs:**
```
User A: Sends message → Message reaches server → Status = SENT
```

**UI Indicator:**
- ✓ (Single checkmark) in sender's message list

---

### 2️⃣ **DELIVERED** (Status = 1)
**Definition:** Message has been successfully delivered to the recipient. The recipient's device has received the push notification or message becomes visible when they open the chat.

**Characteristics:**
- ✅ Message visible to recipient
- ✅ Recipient device received the message (push notification)
- ✅ Message marked as seen in recipient's UI
- ⏳ Waiting for read confirmation from recipient
- 🕐 Server-side delivery timestamp recorded

**When it occurs:**
```
Recipient device: Receives push notification or opens conversation
→ Message fetched by recipient → Status = DELIVERED
```

**UI Indicator:**
- ✓✓ (Double checkmark) in sender's message list
- Message appears in recipient's conversation

**Pusher Event:**
```json
{
  "event": "message.delivered",
  "data": {
    "conversationId": "uuid",
    "messageId": "uuid",
    "deliveredAt": "2026-05-22T10:30:00Z",
    "recipientId": "uuid"
  }
}
```

---

### 3️⃣ **READ** (Status = 2)
**Definition:** Message has been opened and read by the recipient. This is the final state confirming message consumption.

**Characteristics:**
- ✅ Message explicitly marked as read by recipient
- ✅ Full message consumption confirmed
- ✅ Read timestamp available
- ✅ No further state changes
- 🕐 Precise read timestamp recorded

**When it occurs:**
```
Recipient: Opens message conversation → Message visible → System marks as READ
→ Sends read event to server → Status = READ
```

**UI Indicator:**
- ✓✓ (Double checkmark in blue/highlighted) in sender's message list
- Message shows read receipt in detailed view

**Pusher Event:**
```json
{
  "event": "message.read",
  "data": {
    "conversationId": "uuid",
    "messageId": "uuid",
    "readAt": "2026-05-22T10:31:00Z",
    "readBy": "uuid"
  }
}
```

---

## 🔄 State Transitions

```
┌─────────────────────────────────────────────────────────────┐
│              Message Lifecycle State Machine                 │
└─────────────────────────────────────────────────────────────┘

                         SENT (0)
                            |
                       (Push notif)
                            |
                            ↓
                      DELIVERED (1)
                            |
                    (User opens chat)
                            |
                            ↓
                         READ (2)
                            |
                        [Final]
```

**Important Notes:**
- ⚠️ States are **one-way only** - no going backwards
- ⚠️ **SENT → DELIVERED** is automatic via push notifications
- ⚠️ **DELIVERED → READ** requires explicit user action or app interaction
- ⚠️ State skipping can occur (SENT → READ directly if recipient reads immediately)

---

## 🔊 Real-Time Event Handling

### Event Flow Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    Client Application                         │
│  (Flutter, Web, Mobile)                                       │
└─────────────────────────┬──────────────────────────────────────┘
                          │
                    Sends message
                          │
                          ↓
┌──────────────────────────────────────────────────────────────┐
│              ClinicHub API Server                             │
│  POST /api/v1/conversations/{id}/messages                    │
└─────────────────────────┬──────────────────────────────────────┘
                          │
                   Store in DB
                   Status = SENT
                          │
                          ↓
         ┌────────────────────────────────┐
         │  Broadcast via Pusher         │
         │  Event: "message.sent"        │
         └────────────────────────────────┘
                          │
         ┌────────────────┴─────────────────┐
         │                                  │
         ↓                                  ↓
    Sender Device              Recipient Device
    Updates UI                  Shows notification
    ✓ appears                   Receives push
```

### Pusher Event Subscriptions

**Sender Side (Real-time Acknowledgment):**
```javascript
pusher.subscribe(`user-${userId}`).bind('message.sent', (data) => {
  // Message successfully sent to server
  // Update UI: Show ✓
  console.log('Message sent:', data.messageId);
});

pusher.subscribe(`user-${userId}`).bind('message.delivered', (data) => {
  // Message delivered to recipient
  // Update UI: Show ✓✓
  console.log('Message delivered to:', data.recipientId);
});

pusher.subscribe(`user-${userId}`).bind('message.read', (data) => {
  // Message read by recipient
  // Update UI: Show ✓✓ (highlighted/blue)
  console.log('Message read by:', data.readBy);
});
```

**Recipient Side (Real-time Updates):**
```javascript
pusher.subscribe(`conversation-${conversationId}`).bind('message.sent', (data) => {
  // New message received
  // Add to conversation
  // Show notification
  console.log('New message from:', data.senderId);
});
```

---

## 🛠️ Implementation Guide

### Scenario 1: Sending a Message

**Step 1: Client sends message**
```csharp
POST /api/v1/conversations/{conversationId}/messages
{
  "content": "Hello, how are you?",
  "replyToMessageId": null,
  "media": []
}
```

**Step 2: Server processes**
```csharp
// Command Handler
public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Content = request.Content,
            Status = MessageStatus.Sent,  // ← Initial status
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        
        await _unitOfWork.MessageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();
        
        // Broadcast to Pusher
        await _pusherService.TriggerEvent(
            $"conversation-{request.ConversationId}",
            "message.sent",
            new { 
                messageId = message.Id,
                senderId = message.SenderId,
                content = message.Content,
                status = message.Status
            }
        );
        
        return Result<MessageDto>.Success(_mapper.Map<MessageDto>(message));
    }
}
```

**Step 3: Recipients receive in real-time**
```dart
// Flutter Implementation
Pusher.instance.subscribe(
  channelName: 'conversation-$conversationId',
  onEvent: (event) {
    if (event.eventName == 'message.sent') {
      setState(() {
        messages.add(Message.fromJson(event.data));
      });
      // Show notification
      showNotification('New message from ${event.data['senderName']}');
    }
  },
);
```

---

### Scenario 2: Marking Message as Delivered

**Step 1: Recipient device receives message**
```
Push notification → Recipient opens app → Conversation displayed
```

**Step 2: Client marks as delivered**
```csharp
POST /api/v1/conversations/{conversationId}/messages/{messageId}/mark-delivered
```

**Step 3: Server updates status**
```csharp
public class MarkMessageAsDeliveredCommandHandler : IRequestHandler<MarkMessageAsDeliveredCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(MarkMessageAsDeliveredCommand request, CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.MessageRepository.GetByIdAsync(request.MessageId);
        
        if (message.Status == MessageStatus.Sent)  // Only update if still SENT
        {
            message.Status = MessageStatus.Delivered;
            message.DeliveredAt = DateTime.UtcNow;
            
            await _unitOfWork.SaveChangesAsync();
            
            // Broadcast to sender
            await _pusherService.TriggerEvent(
                $"user-{message.SenderId}",
                "message.delivered",
                new { 
                    messageId = message.Id,
                    deliveredAt = message.DeliveredAt,
                    recipientId = request.UserId
                }
            );
        }
        
        return Result<Unit>.Success(Unit.Value);
    }
}
```

---

### Scenario 3: Marking Message as Read

**Step 1: User actively reads message**
```
User taps on conversation → Message becomes visible → System detects
```

**Step 2: Client marks as read**
```csharp
POST /api/v1/conversations/{conversationId}/messages/{messageId}/mark-read
```

**Step 3: Server updates status**
```csharp
public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.MessageRepository.GetByIdAsync(request.MessageId);
        
        if (message.Status != MessageStatus.Read)  // Only update if not already read
        {
            message.Status = MessageStatus.Read;
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            
            await _unitOfWork.SaveChangesAsync();
            
            // Broadcast to sender
            await _pusherService.TriggerEvent(
                $"user-{message.SenderId}",
                "message.read",
                new { 
                    messageId = message.Id,
                    readAt = message.ReadAt,
                    readBy = request.UserId
                }
            );
        }
        
        return Result<Unit>.Success(Unit.Value);
    }
}
```

---

## 🚨 Troubleshooting

### Issue: Message stuck in SENT status

**Possible Causes:**
1. Push notification delivery failed
2. Recipient device offline
3. Network connectivity issues

**Solution:**
```csharp
// Implement auto-retry mechanism
public async Task RetryMessageDelivery(Guid messageId)
{
    var message = await _unitOfWork.MessageRepository.GetByIdAsync(messageId);
    
    if (message.Status == MessageStatus.Sent && 
        DateTime.UtcNow - message.CreatedAt > TimeSpan.FromMinutes(5))
    {
        // Resend push notification
        await _notificationService.SendPushAsync(message.ReceiverId, message);
    }
}
```

---

### Issue: Duplicate message reads

**Possible Causes:**
1. Multiple read events from same user
2. Network retries
3. Client sending multiple read requests

**Solution:**
```csharp
// Idempotent operation check
if (message.Status != MessageStatus.Read)
{
    message.Status = MessageStatus.Read;
    message.ReadAt = DateTime.UtcNow;
    await _unitOfWork.SaveChangesAsync();
}
```

---

### Issue: Unread count not updating

**Possible Causes:**
1. Message status not updated in database
2. Client cache not invalidated
3. Query filter excluding messages

**Solution:**
```csharp
// Ensure query includes unread messages
public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId)
{
    return await _context.Messages
        .Where(m => m.ConversationId == conversationId && 
                    m.RecipientId == userId &&
                    m.Status != MessageStatus.Read &&
                    !m.IsDeleted)
        .CountAsync();
}
```

---

## 📈 Message Flow Diagram

```
TIME →

SENDER                  SERVER                  RECIPIENT
  │                       │                         │
  ├─ Send Message ───────>│                         │
  │                       ├─ Store (SENT)          │
  │                       ├─ Pusher: sent ───────>│
  │<───── Response ────────┤                        │
  │                       │                    Show Notification
  │                       │                        │
  │                       │<─── Push Received ──────┤
  │                       │                    Open Conversation
  │                       │<── Mark Delivered ──────┤
  │                       ├─ Update (DELIVERED)    │
  │                       ├─ Pusher: delivered ──>│
  │<── Read Receipt ──────┤                        │
  │                       │                    Message Visible
  │                       │<──── Mark Read ────────┤
  │                       ├─ Update (READ)         │
  │                       ├─ Pusher: read ───────>│
  │<── Read Receipt ──────┤                        │
  │                    Message Read!
```

---

## 📝 Summary Table

| Scenario | Status | Sender UI | Recipient UI | Event | DB Update |
|----------|--------|-----------|--------------|-------|-----------|
| Message sent | SENT | ✓ | Hidden | sent | Yes |
| Delivered | DELIVERED | ✓✓ | Notification | delivered | Yes |
| Message read | READ | ✓✓ (Blue) | Message visible | read | Yes |

---

## 🔗 Related Documentation

- [REALTIME_FLUTTER_GUIDE.md](./REALTIME_FLUTTER_GUIDE.md) - Flutter integration guide
- [API Routes](./ClinicHub.API/Routes/ApiRoutes.cs) - Available endpoints
- [Message Entity](./ClinicHub.Domain/Entities/Message.cs) - Data model

---

**Last Updated:** May 22, 2026  
**Maintained By:** ClinicHub Development Team
