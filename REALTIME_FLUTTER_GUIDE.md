# ClinicHub Real-time Chat Integration Guide (For Flutter Developers)

> **Base URL:** `https://<your-domain>/api/v1`  
> **Auth:** All endpoints require `Authorization: Bearer <TOKEN>` unless stated otherwise.

---

## 🔌 Phase 1 — Setup & Connection

### 1.1 — Authenticate Pusher Channels
Used internally by the Pusher SDK during `pusher.init()`. The SDK calls this automatically.

```
POST /api/v1/realtime/auth
Content-Type: application/x-www-form-urlencoded
```
| Body Field     | Type   | Description               |
|----------------|--------|---------------------------|
| `socket_id`    | string | Provided by Pusher SDK    |
| `channel_name` | string | Provided by Pusher SDK    |

**Response:** Pusher auth token (string JSON)

**Flutter Setup:**
```dart
await pusher.init(
  apiKey: "8313dec338639cb37d40",
  cluster: "eu",
  onAuthorizer: (channelName, socketId, options) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/v1/realtime/auth'),
      headers: {
        'Authorization': 'Bearer $TOKEN',
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: {'socket_id': socketId, 'channel_name': channelName},
    );
    return jsonDecode(response.body);
  },
  onEvent: onPusherEvent,
);
await pusher.connect();
```

---

### 1.2 — Subscribe to Channels
After connecting, subscribe to:
```dart
// Online presence for all users
await pusher.subscribe(channelName: "presence-global");

// Private channel for receiving your own messages & notifications
await pusher.subscribe(channelName: "private-user-${myUserId.toLowerCase()}");
```

---

### 1.3 — Register Connection (Optional)
Explicitly register your Pusher socket with the backend.

```
POST /api/v1/realtime/connect
Content-Type: application/json
```
| Body Field     | Type   | Description            |
|----------------|--------|------------------------|
| `connectionId` | string | Your Pusher `socketId` |

**Response:** `true`

---

## 💬 Phase 2 — Conversation Management

### 2.1 — Get All Conversations (Chat List Screen)
Fetch the paginated list of conversations to display in the chat list.

```
GET /api/v1/conversations?pageNumber=1&pageSize=10
```
| Query Param  | Type | Default |
|--------------|------|---------|
| `pageNumber` | int  | 1       |
| `pageSize`   | int  | 10      |

**Response fields per conversation:**
```json
{
  "id": "guid",
  "isGroup": false,
  "initiatorId": "guid",
  "initiatorName": "string",
  "initiatorProfilePictureUrl": "string",
  "recipientId": "guid",
  "recipientName": "string",
  "recipientProfilePictureUrl": "string",
  "lastMessageContent": "string",
  "lastMessageDate": "datetime",
  "unreadMessageCount": 3,
  "createdAt": "datetime"
}
```

---

### 2.2 — Create New Conversation
Start a new 1-to-1 conversation with another user.

```
POST /api/v1/conversations/create
Content-Type: application/json
```
| Body Field    | Type | Description                   |
|---------------|------|-------------------------------|
| `recipientId` | Guid | The userId of the other person |

**Response:** `Guid` — the new `conversationId`

---

### 2.3 — Delete Conversation

```
DELETE /api/v1/conversations/{id}
```

**Response:** success message string

---

## 📨 Phase 3 — Chat Screen

### 3.1 — Open a Conversation (do BOTH simultaneously)

#### A) Get Conversation Detail + Message History
Fetches conversation info and **all messages**. Also auto-marks received messages as **Read**.

```
GET /api/v1/conversations/{id}
```

**Response:**
```json
{
  "id": "guid",
  "initiatorId": "guid",
  "initiatorName": "string",
  "initiatorProfilePictureUrl": "string",
  "recipientId": "guid",
  "recipientName": "string",
  "recipientProfilePictureUrl": "string",
  "lastMessageContent": "string",
  "lastMessageDate": "datetime",
  "createdAt": "datetime",
  "messages": [ /* MessageDto[] — see below */ ]
}
```

#### B) Set Active Conversation (REQUIRED for unread counter)
Tell the server the user is now inside this chat screen.

```
POST /api/v1/realtime/active-conversation
Content-Type: application/json
```
| Body Field       | Type    | Description              |
|------------------|---------|--------------------------|
| `conversationId` | Guid?   | The opened conversation  |

> ⚠️ **When the user presses Back**, call this again with `"conversationId": null`!

---

### 3.2 — Get Messages Paginated (Lazy Load / Scroll Up)
Use this for loading older messages on scroll.

```
GET /api/v1/conversations/{conversationId}/messages?pageNumber=1&pageSize=50
```
| Query Param  | Type | Default |
|--------------|------|---------|
| `pageNumber` | int  | 1       |
| `pageSize`   | int  | 50      |

**MessageDto Response Shape:**
```json
{
  "id": "guid",
  "senderId": "guid",
  "senderName": "string",
  "senderProfilePictureUrl": "string",
  "content": "string",
  "isRead": true,
  "readAt": "datetime",
  "status": "Sent | Delivered | Read",
  "createdAt": "datetime",
  "editedAt": "datetime",
  "isEdited": false,
  "conversationId": "guid",
  "replyToMessageId": "guid",
  "replyToMessage": { "id": "...", "senderName": "...", "content": "..." },
  "media": [ { "id": "...", "mediaType": "Image|Video|Audio|File", "fileName": "..." } ],
  "reactions": [ { "id": "...", "userId": "...", "userName": "...", "reactionType": "..." } ]
}
```

---

### 3.3 — Send a Message
```
POST /api/v1/conversations/{conversationId}/messages
Content-Type: application/json
```
| Body Field  | Type   | Description          |
|-------------|--------|----------------------|
| `content`   | string | The message text     |

**Response:** `MessageDto` (the saved message)

> 💡 **Best Practice:** Add the message to the UI immediately (Optimistic Update), then confirm with the API response.

---

### 3.4 — Delete a Message

```
DELETE /api/v1/conversations/messages/{messageId}
```

**Response:** success message string

---

### 3.5 — Typing Indicator
Notify the other user that you are typing.

```
POST /api/v1/realtime/typing
Content-Type: application/json
```
| Body Field       | Type   | Description                         |
|------------------|--------|-------------------------------------|
| `conversationId` | Guid   | The current conversation            |
| `isTyping`       | bool   | `true` when typing, `false` to stop |

> 💡 **Best Practice:** Use a debounce Timer. Send `isTyping: true` on keystroke. Send `isTyping: false` after 2s of silence.

---

## 🔔 Phase 4 — Real-time Pusher Events

Listen in your `onPusherEvent` callback on `private-user-{myUserId}` channel:

| Event Name             | When Triggered                                    | What To Do in Flutter                                           |
|------------------------|---------------------------------------------------|-----------------------------------------------------------------|
| `new-message`          | You receive a new message                         | Append to ListView if chat open, else show notification + increment unread count |
| `conversation-updated` | A conversation's last message changed             | Move conversation to top of list, update snippet & date         |
| `typing`               | Other user started/stopped typing                 | Show/hide "typing..." indicator (`conversationId`, `userId`, `isTyping`) |
| `messages-read`        | Other user opened your chat and read your messages | Change ✓ to ✓✓ for all sent messages in that conversation       |

**`presence-global` channel events:**

| Event Name                    | What To Do                                   |
|-------------------------------|----------------------------------------------|
| `pusher:subscription_succeeded` | Get full list of online users, show green dots |
| `pusher:member_added`         | Mark that user as Online                     |
| `pusher:member_removed`       | Mark that user as Offline                    |

---

## 🔍 Phase 5 — Optional / On-Demand

### 5.1 — Get Online Users (API fallback)
If Pusher presence data isn't enough, fetch online users manually.

```
GET /api/v1/realtime/online-users
```
**Response:** `Guid[]` — list of online userIds

---

### 5.2 — Get Typing Users in a Conversation
Check who is currently typing (useful on screen open).

```
GET /api/v1/realtime/typing/{conversationId}
```
**Response:** `Guid[]` — list of userIds currently typing

---

### 5.3 — Search Users (to start a new chat)
Find a user before creating a conversation.

```
GET /api/v1/auth/users/search?query=...
```

---

### 5.4 — Disconnect (on Logout)
Tell the backend the user is fully offline.

```
POST /api/v1/realtime/disconnect
Content-Type: application/json
```
| Body Field     | Type   | Description            |
|----------------|--------|------------------------|
| `connectionId` | string | Your Pusher `socketId` |

> ⚠️ On disconnect, the backend automatically clears the user's active conversation and typing state.

---

## 📋 Quick Endpoint Reference

| # | Method   | Endpoint                                               | When To Call                              |
|---|----------|--------------------------------------------------------|-------------------------------------------|
| 1 | `POST`   | `/realtime/auth`                                       | Automatically by Pusher SDK               |
| 2 | `POST`   | `/realtime/connect`                                    | After Pusher connects (optional)          |
| 3 | `GET`    | `/conversations`                                       | Load chat list screen                     |
| 4 | `POST`   | `/conversations/create`                                | Start a new conversation                  |
| 5 | `GET`    | `/conversations/{id}`                                  | Open chat screen (marks messages as read) |
| 6 | `POST`   | `/realtime/active-conversation` `{conversationId}`     | Enter chat screen                         |
| 7 | `POST`   | `/realtime/active-conversation` `{null}`               | Leave chat screen (back button)           |
| 8 | `GET`    | `/conversations/{conversationId}/messages`             | Lazy load older messages                  |
| 9 | `POST`   | `/conversations/{conversationId}/messages`             | Send a message                            |
|10 | `DELETE` | `/conversations/messages/{messageId}`                  | Delete a message                          |
|11 | `DELETE` | `/conversations/{id}`                                  | Delete a conversation                     |
|12 | `POST`   | `/realtime/typing`                                     | User is typing / stopped typing           |
|13 | `GET`    | `/realtime/typing/{conversationId}`                    | Get who is typing (on screen open)        |
|14 | `GET`    | `/realtime/online-users`                               | Get online users (API fallback)           |
|15 | `GET`    | `/auth/users/search`                                   | Search users to start a new chat          |
|16 | `POST`   | `/realtime/disconnect`                                 | User logs out                             |

---

## ⚡ TL;DR Summary

**الخطوات الأساسية لتشغيل الـ Chat:**

1. **تسجيل الدخول** → هيئ Pusher واشترك في `presence-global` و `private-user-{userId}`
2. **شاشة المحادثات** → `GET /conversations` لجلب القائمة
3. **محادثة جديدة** → `POST /conversations/create` بـ `recipientId`
4. **فتح المحادثة** → `GET /conversations/{id}` + `POST /active-conversation {id}` معاً
5. **إرسال رسالة** → `POST /conversations/{id}/messages` + أضفها للـ UI فوراً
6. **الكتابة** → `POST /realtime/typing` مع debounce timer
7. **الخروج من المحادثة** → `POST /active-conversation {null}`
8. **تسجيل الخروج** → `POST /realtime/disconnect`
9. **استقبال أي شيء** → Pusher events: `new-message`, `typing`, `messages-read`, `conversation-updated`
