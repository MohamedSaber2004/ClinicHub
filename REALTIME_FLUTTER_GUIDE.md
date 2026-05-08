# ClinicHub Real-time Chat Integration Guide (For Flutter Developers)

This document provides a step-by-step guide on how to integrate the real-time chat functionality using Pusher and the ClinicHub API in your Flutter application.

## 🛠️ Prerequisites
Ensure you are using the official Pusher Channels package for Flutter:
```yaml
dependencies:
  pusher_channels_flutter: ^x.x.x
```

---

## 📖 Step 1: Initialize Pusher & Authentication

When the user logs in and you have their `Bearer Token` and `UserId`, initialize Pusher.

### Endpoint: `POST /api/v1/realtime/auth`
Pusher requires an authentication endpoint to subscribe to `private-` and `presence-` channels.

**Flutter Pusher Setup Example:**
```dart
PusherChannelsFlutter pusher = PusherChannelsFlutter.getInstance();
await pusher.init(
  apiKey: "8313dec338639cb37d40", // Replace with actual key
  cluster: "eu",
  onAuthorizer: (String channelName, String socketId, dynamic options) async {
    // Call our backend Auth endpoint
    final response = await http.post(
      Uri.parse('$baseUrl/api/v1/realtime/auth'),
      headers: {
        'Authorization': 'Bearer $TOKEN',
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: {
        'socket_id': socketId,
        'channel_name': channelName,
      },
    );
    return jsonDecode(response.body);
  },
  onEvent: onPusherEvent, // Handle incoming events here
);
await pusher.connect();
```

---

## 📖 Step 2: Subscribe to Channels

Once connected, you must subscribe to two main channels:

1. **Global Presence Channel (`presence-global`)**: 
   Used to track who is currently online in the app.
   ```dart
   await pusher.subscribe(channelName: "presence-global");
   ```
2. **Private User Channel (`private-user-{YOUR_USER_ID}`)**:
   Used to receive private messages and notifications. *(Note: Convert your User ID to lowercase if required by your backend configuration).*
   ```dart
   await pusher.subscribe(channelName: "private-user-${myUserId.toLowerCase()}");
   ```

---

## 📖 Step 3: Handle App Startup (Crucial for Unread Count)

When the app starts or the user navigates to the Home/Chats screen, you **must** inform the server that the user is NOT inside any specific chat room.

### Endpoint: `POST /api/v1/realtime/active-conversation`
- **Body**: `{ "conversationId": null }`
- **Why?** If the app was closed abruptly previously, the server might still think the user is reading a chat. Sending `null` clears this state so unread counters work correctly.

---

## 📖 Step 4: Fetch Conversations List

### Endpoint: `GET /api/v1/conversations`
Call this endpoint to display the list of chats in your messages screen.
- The response includes `lastMessageContent`, `lastMessageDate`, and `unreadMessageCount`.

---

## 📖 Step 5: Opening a Conversation (Chat Screen)

When the user taps on a conversation to open the Chat Screen, you need to do **two** things simultaneously:

### 1. Fetch the Chat History & Mark as Read
### Endpoint: `GET /api/v1/conversations/{conversationId}`
- Fetches all messages.
- **Backend Behavior**: Calling this endpoint automatically marks all unread messages from the other user as `Read` in the database.

### 2. Set the Active Conversation
### Endpoint: `POST /api/v1/realtime/active-conversation`
- **Body**: `{ "conversationId": "THE_CHAT_ID" }`
- **Why?** This tells the server: *"The user is currently staring at this chat screen."* If the other person sends a message right now, the server will immediately mark it as `Read` (and the unread counter won't increase).

> **⚠️ IMPORTANT:** When the user presses the "Back" button to leave the Chat Screen, you **MUST** call `POST /api/v1/realtime/active-conversation` with `null` again!

---

## 📖 Step 6: Sending a Message

### Endpoint: `POST /api/v1/conversations/{conversationId}/messages`
- **Body**: `{ "content": "Hello there!" }`
- **Action**: Append the message locally to your Flutter UI immediately (optimistic UI update), then call the API.

---

## 📖 Step 7: Typing Indicators

When the user starts typing, let the other person know.

### Endpoint: `POST /api/v1/realtime/typing`
- **Body**: `{ "conversationId": "THE_CHAT_ID", "isTyping": true }`

> **Best Practice in Flutter:** Use a `Timer` (debounce). When the user types, send `isTyping: true`. If the user stops typing for 2 seconds, send `isTyping: false`.

---

## 📖 Step 8: Listening to Real-time Events (Pusher Callbacks)

In your `onPusherEvent` callback, listen to the following event names on your `private-user-{id}` channel:

### 1. `new-message`
- **Triggered when:** You receive a message.
- **Payload:** Message object (content, senderId, conversationId, etc.)
- **Action:** 
  - If the user is currently on the Chat Screen for this `conversationId`, append the message to the ListView.
  - If the user is elsewhere, show a local push notification or update the unread counter.

### 2. `conversation-updated`
- **Triggered when:** A conversation's last message is updated.
- **Payload:** `{ "conversationId": "...", "lastMessage": "...", "lastMessageDate": "..." }`
- **Action:** Update the Conversation List UI to move this chat to the top and update its snippet/date.

### 3. `typing`
- **Triggered when:** The other user is typing.
- **Payload:** `{ "conversationId": "...", "userId": "...", "isTyping": true/false }`
- **Action:** Show or hide a "User is typing..." indicator in the UI.

### 4. `messages-read`
- **Triggered when:** The other user opens the chat and reads your sent messages.
- **Payload:** `{ "conversationId": "..." }`
- **Action:** Update the UI of your sent messages in this conversation. Change the single checkmark (✓) to double checkmarks (✓✓) to indicate they were read.

### 5. `presence-global` channel events
- `pusher:subscription_succeeded`: Provides a list of all currently online members.
- `pusher:member_added`: Triggered when someone opens the app.
- `pusher:member_removed`: Triggered when someone closes the app.
- **Action:** Use this to show the green "Online" dot next to users' avatars.

---

## 📖 Step 9: Fetching Online Users Manually (Optional)

If you don't want to rely solely on the Pusher `presence-global` channel to get the online users (or if you need to fetch the list of online users on demand via API):

### Endpoint: `GET /api/v1/realtime/online-users`
- **Returns:** A list of `userId`s (GUIDs) who are currently connected to the chat server.
- **Action:** Use this list to display a "currently active" list or mark users as online in the UI without relying entirely on Pusher callbacks.

---

## 📖 Step 10: Explicit Connection Management (Optional)

While Pusher webhooks automatically handle most connection tracking behind the scenes, you may sometimes want to explicitly tell the backend that the user is online or offline, particularly if you're managing background sockets or implementing manual Connect/Disconnect buttons.

### Endpoint: `POST /api/v1/realtime/connect`
- **Body:** `{ "connectionId": "YOUR_PUSHER_SOCKET_ID" }`
- **Action:** Explicitly registers the user's connection in the server's memory. This is implicitly done during Auth, but you can call this manually if needed.

### Endpoint: `POST /api/v1/realtime/disconnect`
- **Body:** `{ "connectionId": "YOUR_PUSHER_SOCKET_ID" }`
- **Action:** Explicitly removes the user's connection. 
- **Important:** If a user completely disconnects (all connections closed), the backend automatically wipes their `Active Conversation` and `Typing` states, ensuring they don't remain "stuck" reading a chat or typing while offline. Call this endpoint when the user explicitly clicks "Log Out".
