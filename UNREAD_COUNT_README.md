# Unread Count Management & Scenarios

**Version:** 1.0  
**Last Updated:** May 2026  
**Language:** العربية و English

---

## 📌 Table of Contents

1. [Overview](#overview)
2. [How Unread Count Works](#how-unread-count-works)
3. [Normal Message Scenarios](#normal-message-scenarios)
4. [Media-Only Message Scenarios](#media-only-message-scenarios)
5. [State Transitions](#state-transitions)
6. [Unread Count Algorithms](#unread-count-algorithms)
7. [Real-Time Updates](#real-time-updates)
8. [Implementation Details](#implementation-details)
9. [Troubleshooting](#troubleshooting)

---

## 🎯 Overview

The ClinicHub messaging system maintains accurate unread counts for:
- **Normal Messages**: Text-based messages with optional media
- **Media-Only Messages**: Messages containing only media (no text content)

Both message types contribute to the total unread count and are handled consistently by the system.

| Metric | Definition |
|--------|-----------|
| **Unread Count** | Total number of messages with Status ≠ READ in a conversation |
| **Unread Message** | Message with `Status = SENT` or `Status = DELIVERED` (not READ) |
| **Read Message** | Message with `Status = READ` or `IsRead = true` |

---

## 📊 How Unread Count Works

### Core Principle

```
Unread Count = Total Messages - Read Messages
Unread Count = COUNT(Messages WHERE Status != MessageStatus.Read)
```

### Database Query

```sql
SELECT COUNT(*) as UnreadCount
FROM Messages
WHERE ConversationId = @conversationId
  AND RecipientId = @recipientId
  AND Status != 2  -- MessageStatus.Read = 2
  AND IsDeleted = 0
```

### Key Characteristics

- ✅ **Automatic Calculation**: No manual tracking needed
- ✅ **Consistent**: Same logic for all message types
- ✅ **Media-Agnostic**: Works regardless of content or media presence
- ✅ **Real-Time**: Updates immediately on status changes
- ✅ **Soft-Delete Aware**: Excludes deleted messages

---

## 💬 Normal Message Scenarios

### Scenario 1: Send and Receive Normal Message

**Initial State:**
```
Recipient Unread Count = 0
```

**Step 1: Sender sends message with text**
```json
POST /api/v1/conversations/{conversationId}/messages
{
  "content": "Hello, are you there?",
  "replyToMessageId": null,
  "media": []
}
```

**Step 2: Server creates message**
```csharp
var message = new Message
{
    Id = Guid.NewGuid(),
    ConversationId = conversationId,
    SenderId = senderId,
    RecipientId = recipientId,
    Content = "Hello, are you there?",
    Status = MessageStatus.Sent,        // ← SENT initially
    IsRead = false,
    CreatedAt = DateTime.UtcNow
};

await _unitOfWork.MessageRepository.AddAsync(message);
await _unitOfWork.SaveChangesAsync();
```

**Step 3: Recipient unread count updates**
```
Recipient Unread Count = 1
(Message with Status = SENT is counted)
```

**Database State:**
```sql
SELECT COUNT(*) FROM Messages 
WHERE ConversationId = 'conv-123'
  AND RecipientId = 'user-456'
  AND Status != 2
-- Result: 1
```

**Step 4: Message marked as delivered**
```
POST /api/v1/conversations/{conversationId}/messages/{messageId}/mark-delivered
```

```csharp
message.Status = MessageStatus.Delivered;
message.DeliveredAt = DateTime.UtcNow;
await _unitOfWork.SaveChangesAsync();
```

**Step 5: Unread count remains the same**
```
Recipient Unread Count = 1
(Status = DELIVERED is still unread)
```

**Step 6: Message marked as read**
```
POST /api/v1/conversations/{conversationId}/messages/{messageId}/mark-read
```

```csharp
message.Status = MessageStatus.Read;
message.IsRead = true;
message.ReadAt = DateTime.UtcNow;
await _unitOfWork.SaveChangesAsync();
```

**Step 7: Unread count decreases**
```
Recipient Unread Count = 0
(Message with Status = READ is excluded from count)
```

---

### Scenario 2: Multiple Normal Messages

**Step 1: Sender sends 3 messages**
```json
Message 1: "Hi there" → Status = SENT
Message 2: "How are you?" → Status = SENT
Message 3: "Let me know when you're free" → Status = SENT
```

**Recipient Unread Count = 3**
```sql
SELECT COUNT(*) FROM Messages 
WHERE ConversationId = 'conv-123'
  AND Status != 2
-- Result: 3
```

**Step 2: Recipient reads first message**
```
Message 1: Status = READ
Message 2: Status = DELIVERED
Message 3: Status = DELIVERED
```

**Recipient Unread Count = 2**
```sql
SELECT COUNT(*) FROM Messages 
WHERE ConversationId = 'conv-123'
  AND Status != 2
-- Result: 2 (Messages 2 & 3 still unread)
```

**Step 3: Recipient reads all messages**
```
Message 1: Status = READ
Message 2: Status = READ
Message 3: Status = READ
```

**Recipient Unread Count = 0**

**Visual Representation:**
```
Timeline:
─────────────────────────────────────
Sender: "Hi there" ──────────>
                          Recipient: Unread = 1

Sender: "How are you?" ────>
                          Recipient: Unread = 2

Sender: "Let me know..." ──>
                          Recipient: Unread = 3

                    Recipient reads message 1
                          Unread = 2

                    Recipient reads message 2
                          Unread = 1

                    Recipient reads message 3
                          Unread = 0
─────────────────────────────────────
```

---

## 📸 Media-Only Message Scenarios

### Scenario 1: Send Media-Only Message (Image)

**Initial State:**
```
Recipient Unread Count = 0
```

**Step 1: Sender sends media without content**
```json
POST /api/v1/conversations/{conversationId}/messages
{
  "content": "",  // Empty or null - both accepted
  "replyToMessageId": null,
  "media": [
    {
      "mediaType": 0,  // Image
      "fileName": "vacation.jpg"
    }
  ]
}
```

**Server Validation:**
```csharp
public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator(IStringLocalizer<Messages> localizer)
    {
        // Must have either content OR media
        RuleFor(x => x.Request)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || (x.Media != null && x.Media.Any()))
            .WithMessage(localizer["MessageContentOrMediaRequired"]);
    }
}
```

**Step 2: Server creates message**
```csharp
var message = new Message
{
    Id = Guid.NewGuid(),
    ConversationId = conversationId,
    SenderId = senderId,
    RecipientId = recipientId,
    Content = null,  // No text content
    Status = MessageStatus.Sent,
    IsRead = false,
    CreatedAt = DateTime.UtcNow
};

// Media attached separately
await _unitOfWork.MessageRepository.AddAsync(message);
await _unitOfWork.SaveChangesAsync();

// Add media to message
foreach (var mediaDto in request.Media)
{
    var mediaEntity = _mapper.Map<Media>(mediaDto);
    mediaEntity.MessageId = message.Id;
    await _unitOfWork.MediaRepository.AddAsync(mediaEntity);
}
await _unitOfWork.SaveChangesAsync();
```

**Step 3: Recipient unread count updates**
```
Recipient Unread Count = 1
(Media-only message counts as unread)
```

**Database State:**
```sql
SELECT COUNT(*) FROM Messages 
WHERE ConversationId = 'conv-123'
  AND RecipientId = 'user-456'
  AND Status != 2
-- Result: 1 (Media-only message included)

SELECT * FROM Messages WHERE Id = 'msg-123'
/* Result:
Id: msg-123
Content: NULL          ← No text
Status: 0 (SENT)       ← Still unread
IsRead: false
*/

SELECT * FROM Media WHERE MessageId = 'msg-123'
/* Result:
Id: media-456
MessageId: msg-123
MediaType: 0 (Image)
FileName: vacation.jpg
*/
```

**Important:** ✅ Media-only messages are counted the same as text messages

---

### Scenario 2: Send Multiple Media Messages

**Step 1: Sender sends 2 media messages**
```json
Message 1: Image → No text content
Message 2: Video → No text content
Message 3: Text message with attached image
```

**Recipient Unread Count = 3**
```
All three messages counted (regardless of content type)
```

**Step 2: Database verification**
```sql
-- All message types contribute to unread count
SELECT m.Id, m.Content, COUNT(media.Id) as MediaCount, m.Status
FROM Messages m
LEFT JOIN Media media ON m.Id = media.MessageId
WHERE m.ConversationId = 'conv-123'
  AND m.Status != 2
GROUP BY m.Id, m.Content, m.Status

/* Result:
Id          | Content      | MediaCount | Status
msg-1       | NULL         | 1          | 0 (SENT)     ← Media-only
msg-2       | NULL         | 1          | 0 (SENT)     ← Media-only
msg-3       | "Check this" | 1          | 0 (SENT)     ← Text + media
*/

-- Total Unread Count = 3
```

**Step 3: Read first media message**
```
Message 1: Status = READ
Message 2: Status = DELIVERED
Message 3: Status = DELIVERED
```

**Recipient Unread Count = 2**

---

### Scenario 3: Mixed Content Messages

**Step 1: Send mixed messages**
```json
Message 1: Text only
Message 2: Image only
Message 3: Video only
Message 4: Text + Image
Message 5: Text + Video + Image
```

**Query Result:**
```sql
SELECT 
    m.Id,
    CASE WHEN m.Content IS NULL THEN 'Media-Only'
         WHEN COUNT(media.Id) > 0 THEN 'Text + Media'
         ELSE 'Text-Only' END as Type,
    COUNT(media.Id) as MediaCount,
    m.Status
FROM Messages m
LEFT JOIN Media media ON m.Id = media.MessageId
WHERE m.ConversationId = 'conv-123'
  AND m.Status != 2
GROUP BY m.Id, m.Content, m.Status

/* Result:
Id      | Type          | MediaCount | Status
msg-1   | Text-Only     | 0          | 0 (SENT)
msg-2   | Media-Only    | 1          | 0 (SENT)
msg-3   | Media-Only    | 1          | 0 (SENT)
msg-4   | Text + Media  | 1          | 0 (SENT)
msg-5   | Text + Media  | 2          | 0 (SENT)

Total Unread Count = 5
(All message types counted equally)
*/
```

**Important:** ✅ The system treats all message types uniformly for unread counting

---

## 🔄 State Transitions

### Message State Machine for Unread Counting

```
┌─────────────────────────────────────────────────┐
│        Message Status State Machine              │
│         (Unread Count Impact)                    │
└─────────────────────────────────────────────────┘

        ┌─ SENT (0) ─┐
        │  UNREAD    │
        │ Unread++ ◄─┤
        │            │
        ├────────────┤
        │            │
        │ DELIVERED  │
        │ (1)        │
        │ UNREAD     │
        │ Count same │
        │            │
        ├────────────┤
        │            │
        └─► READ (2) ◄─┘
            UNREAD--
            (Excluded)
```

### Unread Count Update Timeline

```
Sender Action          | Message Status | Recipient Unread Count
─────────────────────────────────────────────────────────────
Send message           | SENT (0)       | Unread++
Receive push notif     | SENT (0)       | Same
Mark as delivered      | DELIVERED (1)  | Same (still unread)
Open conversation      | DELIVERED (1)  | Same
Mark as read           | READ (2)       | Unread--
───────────────────────────────────────────────────────────────
```

---

## 🧮 Unread Count Algorithms

### Algorithm 1: Get Unread Count (Single Conversation)

```csharp
public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId)
{
    return await _context.Messages
        .Where(m => m.ConversationId == conversationId
                    && m.RecipientId == userId
                    && m.Status != MessageStatus.Read  // ← Key filter
                    && !m.IsDeleted)
        .CountAsync();
}
```

**Valid for:**
- ✅ Normal text messages
- ✅ Media-only messages
- ✅ Mixed content messages
- ✅ Reply messages
- ✅ All message types

---

### Algorithm 2: Get Unread Count (All Conversations)

```csharp
public async Task<Dictionary<Guid, int>> GetAllUnreadCountsAsync(Guid userId)
{
    var unreadCounts = await _context.Messages
        .Where(m => m.RecipientId == userId
                    && m.Status != MessageStatus.Read
                    && !m.IsDeleted)
        .GroupBy(m => m.ConversationId)
        .Select(g => new { ConversationId = g.Key, Count = g.Count() })
        .ToDictionaryAsync(x => x.ConversationId, x => x.Count);
    
    return unreadCounts;
}
```

**Output Example:**
```
Conversation-1: 3 unread messages
Conversation-2: 0 unread messages
Conversation-3: 5 unread messages
Conversation-4: 1 unread message
```

---

### Algorithm 3: Bulk Mark As Read

```csharp
public async Task MarkConversationAsReadAsync(Guid conversationId, Guid userId)
{
    var unreadMessages = await _context.Messages
        .Where(m => m.ConversationId == conversationId
                    && m.RecipientId == userId
                    && m.Status != MessageStatus.Read
                    && !m.IsDeleted)
        .ToListAsync();
    
    foreach (var message in unreadMessages)
    {
        message.Status = MessageStatus.Read;
        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
    }
    
    await _context.SaveChangesAsync();
    
    // Unread count becomes 0
}
```

**Execution Time:** O(n) where n = unread messages

---

## 🔊 Real-Time Updates

### Real-Time Unread Count Broadcasting

**When message status changes, broadcast to user:**

```csharp
public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, Result<Unit>>
{
    private readonly IPusherService _pusherService;
    
    public async Task<Result<Unit>> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.MessageRepository.GetByIdAsync(request.MessageId);
        
        message.Status = MessageStatus.Read;
        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        
        await _unitOfWork.SaveChangesAsync();
        
        // Calculate new unread count
        var newUnreadCount = await _unitOfWork.MessageRepository
            .GetUnreadCountAsync(message.ConversationId, message.SenderId);
        
        // Broadcast unread count update
        await _pusherService.TriggerEvent(
            $"user-{message.SenderId}",
            "unread-count.updated",
            new
            {
                conversationId = message.ConversationId,
                unreadCount = newUnreadCount,
                messageId = message.Id,
                readAt = message.ReadAt
            }
        );
        
        return Result<Unit>.Success(Unit.Value);
    }
}
```

**Pusher Event Structure:**
```json
{
  "event": "unread-count.updated",
  "channel": "user-123",
  "data": {
    "conversationId": "conv-456",
    "unreadCount": 2,
    "messageId": "msg-789",
    "readAt": "2026-05-22T10:45:00Z"
  }
}
```

### Client-Side Handling

```dart
// Flutter Implementation
Pusher.instance.subscribe(
  channelName: 'user-$userId',
  onEvent: (event) {
    if (event.eventName == 'unread-count.updated') {
      final data = event.data;
      setState(() {
        conversationUnreadCounts[data['conversationId']] = data['unreadCount'];
      });
      
      // Update UI
      updateConversationBadge(data['conversationId'], data['unreadCount']);
    }
  },
);
```

---

## 🔧 Implementation Details

### Database Schema

```sql
CREATE TABLE Messages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ConversationId UNIQUEIDENTIFIER NOT NULL,
    SenderId UNIQUEIDENTIFIER NOT NULL,
    RecipientId UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(MAX) NULL,              -- ← Can be NULL for media-only
    Status INT NOT NULL DEFAULT 0,           -- ← 0=SENT, 1=DELIVERED, 2=READ
    IsRead BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    DeliveredAt DATETIME2 NULL,
    ReadAt DATETIME2 NULL,
    ReplyToMessageId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Messages_Conversations FOREIGN KEY (ConversationId) 
        REFERENCES Conversations(Id)
);

-- Index for unread count queries
CREATE INDEX IX_Messages_UnreadCount 
    ON Messages(ConversationId, RecipientId, Status, IsDeleted);
```

### Entity Configuration

```csharp
public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Content)
            .HasMaxLength(5000)
            .IsRequired(false);  // ← Nullable for media-only messages
        
        builder.Property(x => x.Status)
            .HasConversion<int>();
        
        builder.Property(x => x.IsRead)
            .HasDefaultValue(false);
        
        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);
        
        // Global query filter
        builder.HasQueryFilter(x => !x.IsDeleted);
        
        // Index for performance
        builder.HasIndex(x => new { x.ConversationId, x.RecipientId, x.Status })
            .HasName("IX_Messages_UnreadCount");
        
        // Relationships
        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ConversationId);
    }
}
```

---

## 🚨 Troubleshooting

### Issue: Unread Count Not Updating

**Problem:** Unread count stays at old value after marking message as read.

**Causes:**
1. Database transaction not committed
2. Client cache not invalidated
3. Pusher event not triggered
4. Query filter excluding messages

**Solution:**
```csharp
// Ensure SaveChangesAsync completes
await _unitOfWork.SaveChangesAsync();

// Force recalculate unread count
var unreadCount = await _unitOfWork.MessageRepository
    .GetUnreadCountAsync(conversationId, userId);

// Broadcast updated count
await _pusherService.TriggerEvent(
    $"user-{senderId}",
    "unread-count.updated",
    new { conversationId, unreadCount }
);
```

---

### Issue: Media-Only Messages Not Counted

**Problem:** Messages with only media (no text) don't appear in unread count.

**Causes:**
1. Validation rejecting empty content with media
2. Query filtering by Content != NULL
3. Message not saved to database

**Solution:**
```csharp
// ✅ Correct: Accept media-only messages
RuleFor(x => x.Request)
    .Must(x => !string.IsNullOrWhiteSpace(x.Content) || (x.Media != null && x.Media.Any()))
    .WithMessage("Message must have content or media");

// ✅ Correct: Query ignores Content
var unreadCount = await _context.Messages
    .Where(m => m.ConversationId == conversationId
                && m.Status != MessageStatus.Read
                && !m.IsDeleted)
    .CountAsync();
```

---

### Issue: Duplicate Unread Counts

**Problem:** Getting different unread counts from different queries.

**Causes:**
1. Deleted messages included in count
2. Soft-delete filter not applied globally
3. IsDeleted flag not updated
4. Different users querying same data

**Solution:**
```csharp
// ✅ Apply global query filter
modelBuilder.Entity<Message>()
    .HasQueryFilter(x => !x.IsDeleted);

// ✅ Use same query for consistency
public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId)
{
    return await _context.Messages
        .Where(m => m.ConversationId == conversationId
                    && m.RecipientId == userId
                    && m.Status != MessageStatus.Read
                    && !m.IsDeleted)
        .CountAsync();
}
```

---

### Issue: Performance Degradation with Large Message Counts

**Problem:** Unread count queries become slow as message volume grows.

**Causes:**
1. Missing database index
2. Inefficient query
3. Full table scan

**Solution:**
```sql
-- Add composite index
CREATE INDEX IX_Messages_UnreadCount 
    ON Messages(ConversationId, RecipientId, Status)
    WHERE IsDeleted = 0;

-- Query execution should use index
SELECT COUNT(*) FROM Messages
WHERE ConversationId = @conversationId
  AND RecipientId = @recipientId
  AND Status != 2
  AND IsDeleted = 0
```

---

## 📋 Summary Table

| Scenario | Message Type | Content | Media | Counted? | Status After Read |
|----------|-------------|---------|-------|----------|-------------------|
| Normal text | Text-only | "Hello" | None | ✅ Yes | Read (2) |
| Image only | Media-only | NULL | 1 image | ✅ Yes | Read (2) |
| Video only | Media-only | NULL | 1 video | ✅ Yes | Read (2) |
| Text + Image | Mixed | "Check" | 1 image | ✅ Yes | Read (2) |
| Multiple media | Mixed | NULL | 3 files | ✅ Yes | Read (2) |
| Reply message | Text+Reply | "Agreed" | None | ✅ Yes | Read (2) |
| Deleted message | Any | - | - | ❌ No | - |

---

## 🔗 Related Documentation

- [MESSAGE_STATES_README.md](./MESSAGE_STATES_README.md) - Message delivery states
- [REALTIME_FLUTTER_GUIDE.md](./REALTIME_FLUTTER_GUIDE.md) - Flutter integration
- [Message Entity](./ClinicHub.Domain/Entities/Message.cs) - Data model
- [MessageRepository](./ClinicHub.Infrastructure/Repositories/MessageRepository.cs) - Queries

---

**Last Updated:** May 22, 2026  
**Maintained By:** ClinicHub Development Team
