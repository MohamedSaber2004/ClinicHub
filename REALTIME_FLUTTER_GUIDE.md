# ClinicHub Real-time Chat Integration Guide (For Flutter Developers)

**إصدار:** v2.0  
**آخر تحديث:** مايو 2026  
**الحالة:** 🔴 هناك مشاكل معروفة تحتاج إلى اهتمام

> **Base URL:** `https://<your-domain>/api/v1`  
> **Auth:** All endpoints require `Authorization: Bearer <TOKEN>` unless stated otherwise.

---

## 📋 جدول المحتويات

1. [المشاكل المعروفة](#المشاكل-المعروفة)
2. [الإعداد الأولي](#phase-1--الإعداد-الأولي)
3. [إدارة المحادثات](#phase-2--إدارة-المحادثات)
4. [شاشة المحادثة](#phase-3--شاشة-المحادثة)
5. [أحداث Pusher الفورية](#phase-4--أحداث-pusher-الفورية)
6. [الحالات الخاصة والحلول](#حالات-خاصة-edge-cases)
7. [الخلاصة السريعة](#tldr-summary)

---

## ✅ حالة المشاكل المعروفة

بعد فحص شامل للـ Server Code على تاريخ **21 مايو 2026**:

### ✅ مشكلة #1: Unread Count مع Media - **تم التحقق ✓**

**الوضع:** ✅ **لا توجد مشكلة**

**الحقائق:**
- الـ Server يدعم media-only messages بشكل صحيح ✓
- Validation في `SendMessageCommand` يسمح بـ media بدون content
- منطق عد الرسائل غير المقروءة في `GetConversationsQueryHandler` يفحص `m.Status != MessageStatus.Read`
- `MarkAsRead()` method يحدّث كلاً من `IsRead` و `Status` بشكل متزامن ✓

**الخلاصة:** يمكنك إرسال رسائل media فقط، والـ unread count سيتحدّث بشكل صحيح.

---

### ✅ مشكلة #2: Reply on Message - **تم التحقق ✓**

**الوضع:** ✅ **لا توجد مشكلة**

**الحقائق:**
- `GetConversationById` يحمّل nested `ReplyToMessage` بشكل صحيح (eager loading ✓)
- `GetConversationMessages` يحمّل `ReplyToMessage` و `Media` بشكل صحيح ✓
- DTO mapping في كلا الـ handlers يرجع `replyToMessage` مع البيانات الكاملة:
  - Id, SenderId, SenderName, Content, CreatedAt
- التحقق من null صحيح: `m.ReplyToMessage != null ? ... : null`

**مثال على Response الصحيح:**
```json
{
  "id": "5f8eb382-6fe6-4157-a5be-e17aa3821f8f",
  "content": "اتفق معك تماماً",
  "replyToMessageId": "8f8b80b5-7788-4444-bbbb-cc7f45b5ea10",
  "replyToMessage": {
    "id": "8f8b80b5-...",
    "senderId": "...",
    "senderName": "أحمد",
    "content": "الرسالة الأصلية",
    "createdAt": "2026-05-21T00:00:00Z"
  }
}
```

**الخلاصة:** يمكنك الرد على الرسائل بثقة، وستحصل على بيانات الرسالة المردود عليها كاملة.

---

### ℹ️ ملاحظات مهمة للـ Flutter Developers:

#### 1️⃣ عند الرد على رسالة:
```dart
// لا تحتاج لحفظ البيانات محلياً - الـ Server يرسلها
final response = await http.post(
  Uri.parse('$baseUrl/api/v1/conversations/$conversationId/messages'),
  body: jsonEncode({
    'content': 'رد على الرسالة',
    'replyToMessageId': 'message-id-here', // مرر الـ ID فقط
    // الـ Server سيرجع replyToMessage بكل البيانات
  }),
);
```

#### 2️⃣ عند إرسال media بدون نص:
```dart
// لا تحتاج لإضافة مسافات - الـ Server يقبل media-only
final response = await http.post(
  Uri.parse('$baseUrl/api/v1/conversations/$conversationId/messages'),
  body: jsonEncode({
    'content': '', // أو null - كلاهما يعمل مع media
    'media': [
      {'mediaType': 0, 'fileName': 'photo.png'},
    ],
  }),
);
```

---

## 🔌 Phase 1 — الإعداد الأولي

### 1.1 — مصادقة قنوات Pusher
يستخدم داخلياً من قبل Pusher SDK أثناء `pusher.init()`. يتم استدعاؤه تلقائياً.

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

### 1.2 — الاشتراك في القنوات (Subscribe to Channels)
بعد الاتصال، اشترك في:
```dart
// Online presence لجميع المستخدمين
await pusher.subscribe(channelName: "presence-global");

// قناة خاصة لاستقبال رسائلك والإشعارات
await pusher.subscribe(channelName: "private-user-${myUserId.toLowerCase()}");
```

---

### 1.3 — تسجيل الاتصال (اختياري)
قم بتسجيل socket الخاص بك مع الخادم بشكل صريح.

```
POST /api/v1/realtime/connect
Content-Type: application/json
```
| Body Field     | Type   | Description            |
|----------------|--------|------------------------|
| `connectionId` | string | Your Pusher `socketId` |

**Response:** `true`

---

## 💬 Phase 2 — إدارة المحادثات (Conversation Management)

### 2.1 — الحصول على جميع المحادثات (شاشة قائمة الدردشة)
احصل على قائمة المحادثات المرقمة لعرضها في قائمة الدردشة.

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
  "name": "string or null (for 1-to-1 conversations)",
  "groupPhotoUrl": "string or null",
  "isGroup": false,
  "lastMessageDate": "datetime",
  "lastMessageContent": "string",
  "createdAt": "datetime",
  "initiatorId": "guid",
  "initiatorName": "string",
  "initiatorProfilePictureUrl": "string",
  "recipientId": "guid",
  "recipientName": "string",
  "recipientProfilePictureUrl": "string",
  "participants": [],
  "unreadMessageCount": 0
}
```

---

### 2.2 — إنشاء محادثة جديدة (Create New Conversation)
ابدأ محادثة واحد إلى واحد مع مستخدم آخر.

```
POST /api/v1/conversations/create
Content-Type: application/json
```
| Body Field    | Type | Description                   |
|---------------|------|-------------------------------|
| `recipientId` | Guid | معرّف المستخدم الآخر |

**Response:** `Guid` — معرّف المحادثة الجديدة

---

### 2.3 — حذف المحادثة (Delete Conversation)

```
DELETE /api/v1/conversations/{id}
```

**Response:** رسالة نجاح

---

## 📨 Phase 3 — شاشة المحادثة (Chat Screen)

### 3.1 — فتح محادثة (افعل كلاهما معاً)

#### A) الحصول على تفاصيل المحادثة + سجل الرسائل
احصل على معلومات المحادثة و**جميع الرسائل**. يقوم تلقائياً بوضع علامة على الرسائل المستقبلة بـ **مقروءة**.

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
  "messages": [ /* MessageDto[] */ ]
}
```

#### B) تعيين المحادثة النشطة (مطلوب لعداد غير مقروء)
أخبر السيرفر أنك الآن داخل شاشة هذه الدردشة.

```
POST /api/v1/realtime/active-conversation
Content-Type: application/json
```
| Body Field       | Type    | Description              |
|------------------|---------|--------------------------|
| `conversationId` | Guid?   | المحادثة المفتوحة  |

> ⚠️ **عند الضغط على الخلف (Back)**, استدعِ هذا مرة أخرى مع `"conversationId": null`!

---

### 3.2 — الحصول على الرسائل بشكل مرقّم (تحميل كسول / التمرير لأعلى)
استخدم هذا لتحميل الرسائل القديمة عند التمرير.

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
  "status": "int (0=Pending, 1=Sent, 2=Delivered, 3=Read, 4=Sent)",
  "createdAt": "datetime",
  "editedAt": "datetime or null",
  "isEdited": false,
  "conversationId": "guid",
  "replyToMessageId": "guid or null",
  "replyToMessage": "MessageDto or null (nested reply info)",
  "media": [ { "id": "guid", "mediaType": "int (0=Image, 1=Video, 2=Audio, 3=Document)", "fileName": "string" } ],
  "reactions": [ { "id": "guid", "userId": "guid", "userName": "string", "reactionType": "string" } ]
}
```

---

### 3.3 — إرسال رسالة (مع دعم الرد والوسائط)

> **توضيح التعديلات الجديدة لمطوري الفلاتر:**
> 1. **ميزة الرد (Reply):** لإرسال رد على رسالة معينة، مرّر معرّف `replyToMessageId` الخاص بالرسالة الأصلية.
> 2. **إرسال الوسائط (Media Attachments):**
>    - **الخطوة الأولى:** رفع الملف مسبقاً عبر endpoints الرفع المناسبة مع تحديد `place`:
>      - صور: `place = 9` (MessageImages)
>      - فيديو: `place = 10` (MessageVideos)
>      - ملف: `place = 11` (MessageDocuments)
>      - صوت: `place = 12` (MessageAudio)
>    - **الخطوة الثانية:** سيعود اسم ملف فريد. مرّره في مصفوفة `media` مع `mediaType`:
>      - `0` = صورة (Image)
>      - `1` = فيديو (Video)
>      - `2` = صوت (Audio)
>      - `3` = ملف (Document)

```
POST /api/v1/conversations/{conversationId}/messages
Content-Type: application/json
```

| Body Field          | Type   | Required | Description                                                                                    |
|---------------------|--------|----------|------------------------------------------------------------------------------------------------|
| `content`           | string | Yes      | نص الرسالة.                                                                                    |
| `replyToMessageId`  | Guid?  | No       | معرف الرسالة التي يتم الرد عليها (`null` للرسائل العادية).                             |
| `media`             | List   | No       | مصفوفة من الوسائط (اترك `null` أو فارغة إن لم تكن هناك مرفقات). |

#### كائن الميديا:
| Field       | Type   | Description                                                                    |
|-------------|--------|--------------------------------------------------------------------------------|
| `mediaType` | int    | نوع الميديا: `0` صورة، `1` فيديو، `2` صوت، `3` ملف.     |
| `fileName`  | string | اسم الملف الفريد من Upload Endpoint (مثال: `uuid_filename.png`).  |

#### 📥 أمثلة على Payloads:

**رسالة عادية:**
```json
{
  "content": "نص الرسالة"
}
```

**رسالة رد:**
```json
{
  "content": "رد على الرسالة السابقة",
  "replyToMessageId": "8f8b80b5-7788-4444-bbbb-cc7f45b5ea10"
}
```

**رسالة بصورة ومستند:**
```json
{
  "content": "هذا هو التقرير الطبي وصورة الأشعة.",
  "media": [
    {
      "mediaType": 0,
      "fileName": "xray_chest_scan_329.png"
    },
    {
      "mediaType": 3,
      "fileName": "medical_report_patient_12.pdf"
    }
  ]
}
```

#### 📥 مثال على Response (نجح):
```json
{
  "id": "5f8eb382-6fe6-4157-a5be-e17aa3821f8f",
  "senderId": "79c6d40a-d14b-4602-e0f0-08dea31aa277",
  "senderName": "ملن",
  "content": "test",
  "isRead": true,
  "status": 4,
  "createdAt": "2026-05-21T00:20:12.6334217+02:00",
  "media": [],
  "reactions": [],
  "replyToMessageId": null,
  "replyToMessage": null
}
```

> 💡 **Best Practice (Flutter):**
> * أضف الرسالة للـ UI فوراً بشكل مؤقت (Optimistic Update) مع مؤشر جاري الإرسال
> * عند استقبال Response، حدّث حالة الرسالة (Status) والـ ID الفعلي
> * في شاشة المحادثات، للرسائل بـ media فقط اعرض: `"📷 صورة"` أو `"📄 مستند"`

---

### 3.4 — حذف رسالة (Delete a Message)

```
DELETE /api/v1/conversations/messages/{messageId}
```

**Response:** رسالة نجاح

---

### 3.5 — مؤشر الكتابة (Typing Indicator)
أخبر المستخدم الآخر أنك تكتب.

```
POST /api/v1/realtime/typing
Content-Type: application/json
```
| Body Field       | Type   | Description                         |
|------------------|--------|-------------------------------------|
| `conversationId` | Guid   | المحادثة الحالية            |
| `isTyping`       | bool   | `true` عند الكتابة، `false` عند الإيقاف |

> 💡 **Best Practice:** استخدم debounce Timer. أرسل `isTyping: true` عند الكتابة. أرسل `isTyping: false` بعد 2 ثانية من السكوت.

---

## 🔔 Phase 4 — أحداث Pusher الفورية (Real-time Pusher Events)

استمع في `onPusherEvent` على قناة `private-user-{myUserId}`:

| Event Name             | متى يُطلق                                    | ماذا تفعل في Flutter                                           |
|------------------------|---------------------------------------------------|-----------------------------------------------------------------|
| `new-message`          | استقبال رسالة جديدة                         | أضفها للـ ListView إن كانت الشاشة مفتوحة، وإلا أظهر إشعار |
| `conversation-updated` | تحديث آخر رسالة في محادثة             | انقل المحادثة للأعلى، حدّث الملخص والتاريخ         |
| `typing`               | المستخدم الآخر بدأ/توقف الكتابة                 | اعرض/اخفِ مؤشر "يكتب..." |
| `messages-read`        | المستخدم الآخر قرأ رسائلك | غيّر ✓/✓✓ للأزرق ✓✓ |
| `messages-delivered`   | المستخدم الآخر متصل وتسلّم رسائلك  | غيّر ✓ ل ✓✓ |

**أحداث قناة `presence-global`:**

| Event Name                    | ماذا تفعل                                   |
|-------------------------------|----------------------------------------------|
| `pusher:subscription_succeeded` | احصل على قائمة المستخدمين المتصلين |
| `pusher:member_added`         | ضع علامة المستخدم كـ متصل (Online)                     |
| `pusher:member_removed`       | ضع علامة المستخدم كـ غير متصل (Offline)                    |

---

## 🎯 حالات خاصة (Edge Cases)

### Case 1️⃣: إرسال Media بدون نص

**الحل (في Flutter):**
```dart
Future<void> sendMessageWithMedia() async {
  String content = _textController.text.trim();
  
  // إذا كانت هناك media ولا يوجد نص، أضف مسافة
  if (_selectedMedia.isNotEmpty && content.isEmpty) {
    content = " ";
  }
  
  if (content.trim().isEmpty && _selectedMedia.isEmpty) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('اكتب شيء أو أضف صورة')),
    );
    return;
  }

  final payload = {
    'content': content,
    if (_selectedMedia.isNotEmpty) 
      'media': _selectedMedia.map((m) => {
        'mediaType': m.type,
        'fileName': m.fileName,
      }).toList(),
  };

  // أرسل الرسالة...
}

// لعرض معاينة الرسالة
String getMessagePreview(MessageDto message) {
  if ((message.content == null || message.content.trim().isEmpty) && 
      message.media.isNotEmpty) {
    switch (message.media.first.mediaType) {
      case 0: return "📷 صورة";
      case 1: return "🎥 فيديو";
      case 2: return "🎵 صوت";
      case 3: return "📄 ملف";
      default: return "📎 مرفق";
    }
  }
  return message.content ?? "";
}
```

---

### Case 2️⃣: الرد على رسالة (Reply)

**الحل (في Flutter):**
```dart
class MessageReply {
  final String messageId;
  final String content;
  final String senderName;
  
  MessageReply({
    required this.messageId,
    required this.content,
    required this.senderName,
  });
}

class ChatScreenState extends State<ChatScreen> {
  MessageReply? _selectedReply;

  void _selectMessageForReply(MessageDto message) {
    setState(() {
      _selectedReply = MessageReply(
        messageId: message.id,
        content: message.content ?? '',
        senderName: message.senderName,
      );
      _textFieldFocus.requestFocus();
    });
  }

  Future<void> _sendReply() async {
    final payload = {
      'content': _textController.text,
      if (_selectedReply != null) 'replyToMessageId': _selectedReply!.messageId,
    };

    // أرسل الرسالة...
  }

  Widget _buildReplyQuote(MessageReply reply) {
    return Container(
      padding: EdgeInsets.all(12),
      color: Colors.blue.shade50,
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('رد على: ${reply.senderName}',
                    style: TextStyle(fontWeight: FontWeight.bold, color: Colors.blue)),
                SizedBox(height: 4),
                Text(reply.content,
                    maxLines: 1, overflow: TextOverflow.ellipsis),
              ],
            ),
          ),
          IconButton(
            icon: Icon(Icons.close),
            onPressed: () => setState(() => _selectedReply = null),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        children: [
          Expanded(child: _buildMessagesList()),
          if (_selectedReply != null) _buildReplyQuote(_selectedReply!),
          _buildMessageInput(),
        ],
      ),
    );
  }
}
```

---

### Case 3️⃣: معالجة حالات الشبكة والأخطاء

```dart
enum MessageStatus {
  pending,    // قيد الإرسال
  sent,       // تم الإرسال
  delivered,  // تم التسليم
  read,       // تم القراءة
  failed,     // فشل
}

// عند الإرسال، أضف الرسالة مؤقتاً ثم حدّثها
final tempMessage = MessageDto(
  id: 'temp-${DateTime.now().millisecondsSinceEpoch}',
  senderId: currentUserId,
  content: content,
  status: 0, // Pending
  createdAt: DateTime.now(),
);

_messages.add(tempMessage);
notifyListeners();

// بعد الحصول على Response من السيرفر
final index = _messages.indexWhere((m) => m.id == tempMessage.id);
if (index != -1) {
  _messages[index] = realMessage; // استبدلها برد السيرفر
}
notifyListeners();
```

---

### Case 4️⃣: تحديث Unread Count بشكل صحيح

```dart
class ConversationProvider extends ChangeNotifier {
  String? activeConversationId;

  Future<void> setActiveConversation(String conversationId) async {
    activeConversationId = conversationId;
    
    // أخبر السيرفر
    await http.post(
      Uri.parse('$baseUrl/api/v1/realtime/active-conversation'),
      body: jsonEncode({'conversationId': conversationId}),
    );

    // صفّر unread count محلياً
    final convo = conversations.firstWhere((c) => c.id == conversationId);
    convo.unreadMessageCount = 0;
    notifyListeners();
  }

  void onMessageReceived(MessageDto message) {
    if (activeConversationId != message.conversationId) {
      // زيّد العدّاد إذا كانت من محادثة أخرى
      final convo = conversations.firstWhere(
        (c) => c.id == message.conversationId
      );
      convo.unreadMessageCount++;
      notifyListeners();
    }
  }

  Future<void> clearActiveConversation() async {
    await http.post(
      Uri.parse('$baseUrl/api/v1/realtime/active-conversation'),
      body: jsonEncode({'conversationId': null}),
    );
    activeConversationId = null;
  }
}
```

---

### Case 5️⃣: Typing Indicator مع Debounce

```dart
class TypingIndicator {
  Timer? _debounceTimer;
  bool _isTyping = false;

  void onTextChanged(String text) {
    if (text.isNotEmpty && !_isTyping) {
      _sendTypingStatus(true);
      _isTyping = true;
    }

    _debounceTimer?.cancel();
    _debounceTimer = Timer(Duration(seconds: 2), () {
      if (_isTyping) {
        _sendTypingStatus(false);
        _isTyping = false;
      }
    });
  }

  void dispose() {
    _debounceTimer?.cancel();
  }
}
```

---

## 📋 ملخص الأفضليات (Best Practices)

| الممارسة | الفائدة |
|---------|--------|
| **Optimistic Updates** | تحسين تجربة المستخدم |
| **Debounce على Typing** | تقليل الطلبات غير الضرورية |
| **حفظ Local Caching** | سرعة الوصول والعمل بدون إنترنت |
| **Eager Loading** | تجنب Null references |
| **Upload Media مسبقاً** | تقليل فرص فشل الإرسال |
| **Retry Logic** | التعامل مع أخطاء الشبكة |
| **استخدم Streams/Providers** | تحديث الـ UI بفعالية |

---

## ✅ خلاصة التحقق من الـ Server (21 مايو 2026)

**تم فحص شامل لـ:**
- SendMessageCommandHandler - ✅ يدعم media-only messages
- GetConversationsQueryHandler - ✅ عد الرسائل غير المقروءة صحيح
- GetConversationMessagesQueryHandler - ✅ يرجع replyToMessage كاملة
- GetConversationByIdQueryHandler - ✅ eager loading صحيح

**النتيجة:** ✅ **كل المشاكل الموثقة قد تم التحقق من عدم وجودها في الـ Server**

**الاستنتاج:** يمكنك الاستخدام بثقة! 🎉

---

## 🐛 في حالة وجود مشاكل

عند مواجهة مشاكل:
1. افحص **Console Logs** للأخطاء الفعلية
2. استخدم **Postman** لاختبار الـ Endpoints
3. تحقق من **Network Tab** في DevTools
4. في معظم الحالات، المشكلة تكون على جهة الـ Flutter، وليس الـ Server

---

## 📋 Quick Endpoint Reference

| # | Method   | Endpoint                                               | متى تستدعيها                              |
|---|----------|--------------------------------------------------------|-------------------------------------------|
| 1 | `POST`   | `/realtime/auth`                                       | تلقائياً من Pusher SDK               |
| 2 | `POST`   | `/realtime/connect`                                    | بعد اتصال Pusher               |
| 3 | `GET`    | `/conversations`                                       | تحميل قائمة المحادثات                     |
| 4 | `POST`   | `/conversations/create`                                | محادثة جديدة                  |
| 5 | `GET`    | `/conversations/{id}`                                  | فتح المحادثة       |
| 6 | `POST`   | `/realtime/active-conversation` `{conversationId}`     | الدخول للمحادثة                         |
| 7 | `POST`   | `/realtime/active-conversation` `{null}`               | الخروج من المحادثة    |
| 8 | `GET`    | `/conversations/{conversationId}/messages`             | تحميل الرسائل القديمة                 |
| 9 | `POST`   | `/conversations/{conversationId}/messages`             | إرسال رسالة                            |
| 10 | `DELETE` | `/conversations/messages/{messageId}`                  | حذف رسالة                          |
| 11 | `DELETE` | `/conversations/{id}`                                  | حذف المحادثة                     |
| 12 | `POST`   | `/realtime/typing`                                     | يكتب / توقف الكتابة           |
| 13 | `GET`    | `/realtime/typing/{conversationId}`                    | من يكتب        |
| 14 | `GET`    | `/realtime/online-users`                               | المتصلون                           |
| 15 | `GET`    | `/auth/users/search`                                   | البحث عن مستخدم          |
| 16 | `POST`   | `/realtime/disconnect`                                 | تسجيل الخروج                             |

---

## ⚡ TL;DR Summary

**الخطوات الأساسية:**

1. **Login** → Pusher + اشترك في القنوات
2. **Chat List** → `GET /conversations`
3. **New Chat** → `POST /conversations/create`
4. **Open Chat** → `GET /conversations/{id}` + `POST /active-conversation {id}`
5. **Send Message** → `POST /conversations/{id}/messages`
6. **Typing** → `POST /realtime/typing` مع debounce
7. **Exit Chat** → `POST /active-conversation {null}`
8. **Logout** → `POST /realtime/disconnect`
9. **Real-time** → استمع لـ Pusher events

---

## 📝 معلومات الملف

- **تحديث أخير:** 21 مايو 2026
- **الإصدار:** 2.0 - تم التحقق من صحة الـ Server ✅
- **الحالة:** جاهز للاستخدام 🚀
