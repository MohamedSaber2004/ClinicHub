# 🔗 Real-time Connect/Disconnect Flow Guide

**إصدار:** 1.0  
**تاريخ الإنشاء:** مايو 2026  
**الهدف:** شرح مفصل لـ lifecycle الاتصال بـ real-time system

---

## 📋 جدول المحتويات

1. [المراحل الأربع الرئيسية](#المراحل-الأربع-الرئيسية)
2. [مثال عملي Complete Flow](#مثال-عملي-complete-flow)
3. [الفروقات الأساسية](#الفروقات-الأساسية)
4. [أخطاء شائعة وحلولها](#أخطاء-شائعة-وحلولها)
5. [معلومات إضافية](#معلومات-إضافية)

---

## 🚀 المراحل الأربع الرئيسية

### **المرحلة 1️⃣: INITIALIZATION & LOGIN**

**متى:** عند دخول المستخدم للتطبيق  
**الهدف:** تهيئة Pusher والاتصال بـ real-time system

```dart
// 1️⃣ تهيئة Pusher SDK
await pusher.init(
  apiKey: "YOUR_PUSHER_KEY",
  cluster: "eu",
  onAuthorizer: (channelName, socketId, options) async {
    // يستدعي تلقائياً: POST /realtime/auth
    final response = await http.post(
      Uri.parse('$baseUrl/api/v1/realtime/auth'),
      headers: {'Authorization': 'Bearer $TOKEN'},
      body: {'socket_id': socketId, 'channel_name': channelName},
    );
    return jsonDecode(response.body);
  },
);

// 2️⃣ الاتصال الفعلي بـ Pusher
await pusher.connect(); // يولد socket_id جديد

// 3️⃣ الاشتراك في القنوات
await pusher.subscribe(channelName: "presence-global");
await pusher.subscribe(channelName: "private-user-${userId.toLowerCase()}");

// 4️⃣ تسجيل الاتصال مع الخادم (IMPORTANT!)
final connectResponse = await http.post(
  Uri.parse('$baseUrl/api/v1/realtime/connect'),
  headers: {'Authorization': 'Bearer $TOKEN'},
  body: jsonEncode({'connectionId': pusher.getSocketId()}),
);
// Response: { "success": true }
```

**ماذا يحدث في الخادم:**
- ✅ يحفظ `socket_id` + `userId` في قاعدة البيانات
- ✅ يعلم النظام أن المستخدم متصل
- ✅ الآن يمكن الآخرين أن يروا أن المستخدم `online`

**الحالة الحالية:** `🟢 ONLINE`

---

### **المرحلة 2️⃣: ACTIVE CONVERSATION (أثناء الدردشة)**

**متى:** عند فتح محادثة  
**الهدف:** تفعيل real-time events للمحادثة المحددة

```dart
// عند الدخول لمحادثة
await http.post(
  Uri.parse('$baseUrl/api/v1/realtime/active-conversation'),
  headers: {'Authorization': 'Bearer $TOKEN'},
  body: jsonEncode({'conversationId': conversationId}),
);
// Response: { "success": true }
```

**ماذا يحدث في الخادم:**
- ✅ المستخدم متصل ✓
- ✅ يشاهد محادثة معينة ✓
- ✅ يستقبل real-time events من هذه المحادثة فقط ✓

**الرسائل والأحداث المستقبلة:**

```dart
// الرسائل الجديدة
pusher.onEvent('new-message', (event) {
  final message = jsonDecode(event.data);
  // أضف الرسالة للـ UI فوراً
  setState(() => messages.add(message));
});

// حالة الكتابة (Typing Indicator)
pusher.onEvent('user-typing', (event) {
  final typingUser = jsonDecode(event.data);
  // اعرض "Ahmed is typing..."
  setState(() => isTyping[typingUser['userId']] = true);
});

// القراءة (Message Read Receipt)
pusher.onEvent('message-read', (event) {
  final readData = jsonDecode(event.data);
  // حدّث حالة الرسالة
  setState(() => messages[readData['messageId']].isRead = true);
});
```

**الحالة الحالية:** `🟢 ONLINE 👀 WATCHING`

---

### **المرحلة 3️⃣: EXIT CONVERSATION (عند الخروج من المحادثة)**

**متى:** عند الخروج من شاشة المحادثة  
**الهدف:** إيقاف real-time events من هذه المحادثة

```dart
// إخبار الخادم أنك خرجت من المحادثة
await http.post(
  Uri.parse('$baseUrl/api/v1/realtime/active-conversation'),
  headers: {'Authorization': 'Bearer $TOKEN'},
  body: jsonEncode({'conversationId': null}),
);
// Response: { "success": true }
```

**ماذا يحدث في الخادم:**
- ✅ لا تتلقى real-time events من المحادثة
- ✅ لكنك لا تزال `online` في النظام
- ✅ تستقبل إشعارات عن رسائل جديدة (non-real-time)

**الحالة الحالية:** `🟢 ONLINE (لكن لا تشاهد أي محادثة)`

---

### **المرحلة 4️⃣: LOGOUT & DISCONNECT**

**متى:** عند تسجيل الخروج  
**الهدف:** قطع الاتصال بالكامل

```dart
// 1️⃣ إخبار الخادم بقطع الاتصال
await http.post(
  Uri.parse('$baseUrl/api/v1/realtime/disconnect'),
  headers: {'Authorization': 'Bearer $TOKEN'},
  body: jsonEncode({'connectionId': pusher.getSocketId()}),
);
// Response: { "success": true }

// 2️⃣ قطع اتصال Pusher
await pusher.disconnect();
```

**ماذا يحدث في الخادم:**
- ✅ socket_id محذوف من قاعدة البيانات
- ✅ المستخدم `offline` في النظام
- ✅ لا رسائل real-time
- ✅ لا إشعارات

**الحالة الحالية:** `🔴 OFFLINE`

---

## 📊 الفروقات الأساسية

| المرحلة | الـ Endpoint | الـ Body | المقصد | النتيجة |
|--------|-----------|---------|---------|---------|
| **تسجيل الدخول** | `POST /realtime/connect` | `{ "connectionId": "socket_id" }` | ربط socket_id مع user ID | **🟢 Online** |
| **فتح محادثة** | `POST /realtime/active-conversation` | `{ "conversationId": "id" }` | تفعيل real-time للمحادثة | **👀 Watching** |
| **الخروج من المحادثة** | `POST /realtime/active-conversation` | `{ "conversationId": null }` | إيقاف real-time للمحادثة | **✋ Not Watching** |
| **تسجيل الخروج** | `POST /realtime/disconnect` | `{ "connectionId": "socket_id" }` | قطع الاتصال النهائي | **🔴 Offline** |

---

## 📝 مثال عملي (Complete Flow)

```dart
void main() async {
  runApp(const MyApp());
}

class MyApp extends StatefulWidget {
  const MyApp({Key? key}) : super(key: key);

  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  late String token;
  late String userId;

  @override
  void initState() {
    super.initState();
    _initializeApp();
  }

  // ============ STEP 1: LOGIN ============
  Future<void> _initializeApp() async {
    // تسجيل الدخول والحصول على token
    final loginResponse = await login('email@example.com', 'password');
    token = loginResponse['token'];
    userId = loginResponse['userId'];

    // تهيئة Pusher وتسجيل الاتصال
    await _connectToPusher();
  }

  // ============ STEP 2: INITIALIZE PUSHER ============
  Future<void> _connectToPusher() async {
    await pusher.init(
      apiKey: "8313dec338639cb37d40",
      cluster: "eu",
      onAuthorizer: (channelName, socketId, options) async {
        final response = await http.post(
          Uri.parse('$baseUrl/api/v1/realtime/auth'),
          headers: {'Authorization': 'Bearer $token'},
          body: {'socket_id': socketId, 'channel_name': channelName},
        );
        return jsonDecode(response.body);
      },
    );

    await pusher.connect();

    // الاشتراك في القنوات
    await pusher.subscribe(channelName: "presence-global");
    await pusher.subscribe(channelName: "private-user-${userId.toLowerCase()}");

    // تسجيل الاتصال مع الخادم
    await http.post(
      Uri.parse('$baseUrl/api/v1/realtime/connect'),
      headers: {'Authorization': 'Bearer $token'},
      body: jsonEncode({'connectionId': pusher.getSocketId()}),
    );

    print("✅ Connected to real-time system");
  }

  // ============ STEP 3: OPEN CHAT SCREEN ============
  Future<void> _openConversation(String conversationId) async {
    // تفعيل real-time للمحادثة
    await http.post(
      Uri.parse('$baseUrl/api/v1/realtime/active-conversation'),
      headers: {'Authorization': 'Bearer $token'},
      body: jsonEncode({'conversationId': conversationId}),
    );

    print("👀 Watching conversation: $conversationId");
  }

  // ============ STEP 4: LISTEN TO REAL-TIME EVENTS ============
  void _setupRealtimeListeners() {
    pusher.onEvent('new-message', (event) {
      final message = jsonDecode(event.data);
      print("📨 New message received: ${message['content']}");
      // تحديث الـ UI
    });

    pusher.onEvent('user-typing', (event) {
      final data = jsonDecode(event.data);
      print("✍️ ${data['userName']} is typing...");
    });
  }

  // ============ STEP 5: SEND MESSAGE ============
  Future<void> _sendMessage(String conversationId, String content) async {
    await http.post(
      Uri.parse('$baseUrl/api/v1/conversations/$conversationId/messages'),
      headers: {'Authorization': 'Bearer $token'},
      body: jsonEncode({'content': content}),
    );

    print("✅ Message sent");
  }

  // ============ STEP 6: EXIT CONVERSATION ============
  Future<void> _exitConversation() async {
    // إيقاف real-time للمحادثة
    await http.post(
      Uri.parse('$baseUrl/api/v1/realtime/active-conversation'),
      headers: {'Authorization': 'Bearer $token'},
      body: jsonEncode({'conversationId': null}),
    );

    print("✋ Stopped watching conversation");
  }

  // ============ STEP 7: LOGOUT ============
  Future<void> _logout() async {
    // قطع الاتصال
    await http.post(
      Uri.parse('$baseUrl/api/v1/realtime/disconnect'),
      headers: {'Authorization': 'Bearer $token'},
      body: jsonEncode({'connectionId': pusher.getSocketId()}),
    );

    // قطع Pusher
    await pusher.disconnect();

    print("🔴 Disconnected from real-time system");
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ClinicHub Chat',
      home: Scaffold(
        appBar: AppBar(title: const Text('Conversations')),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              ElevatedButton(
                onPressed: () => _openConversation('conv-123'),
                child: const Text('Open Chat'),
              ),
              ElevatedButton(
                onPressed: () => _sendMessage('conv-123', 'Hello!'),
                child: const Text('Send Message'),
              ),
              ElevatedButton(
                onPressed: _exitConversation,
                child: const Text('Exit Chat'),
              ),
              ElevatedButton(
                onPressed: _logout,
                child: const Text('Logout'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
```

---

## 🐛 أخطاء شائعة وحلولها

### **المشكلة 1: لا تستقبل رسائل real-time**

```
❌ الخطأ: 
  - فتحت المحادثة لكن لا تستقبل رسائل جديدة

✅ الحل:
  - تأكد أنك استدعيت POST /realtime/connect بعد Pusher login
  - تأكد من نسخ socket_id الصحيح من Pusher
```

---

### **المشكلة 2: لا تستقبل رسائل المحادثة الحالية**

```
❌ الخطأ:
  - تستقبل رسائل من محادثات أخرى
  - لكن لا تستقبل من المحادثة المفتوحة

✅ الحل:
  - تأكد أنك استدعيت POST /realtime/active-conversation مع conversation ID
  - تأكد أن ال conversationId صحيح
```

---

### **المشكلة 3: تستقبل رسائل من محادثات أخرى عند الخروج**

```
❌ الخطأ:
  - خرجت من محادثة
  - لكن تستقبل رسائل منها في real-time

✅ الحل:
  - استدع POST /realtime/active-conversation مع { "conversationId": null }
  - تأكد أنك لا تسمع للـ events من المحادثة
```

---

### **المشكلة 4: Socket keeps reconnecting / infinite loop**

```
❌ الخطأ:
  - Pusher يحاول الاتصال مرراً وتكراراً

✅ الحل:
  - استدع pusher.connect() مرة واحدة فقط
  - ضعها في initState() أو عند startup
  - لا تستدعها في build() أو setState()
  - استخدم StreamBuilder لـ real-time updates
```

---

### **المشكلة 5: Token expired لكن Pusher لا يزال متصل**

```
❌ الخطأ:
  - token الخاص بك انقضى
  - لكن Pusher socket لا يزال نشط

✅ الحل:
  - استدع /realtime/disconnect قبل logout
  - احذف token القديم
  - أعد تسجيل الدخول
  - استدع /realtime/connect مع token جديد
```

---

## 📌 معلومات إضافية

### **الحالات الخاصة:**

#### **1. المستخدم له عدة أجهزة متصلة:**

```dart
// كل جهاز يأخذ socket_id مختلف
// الخادم يتعامل مع كل واحد بشكل منفصل

// الجهاز 1:
POST /realtime/connect
Body: { "connectionId": "socket_id_1" }

// الجهاز 2:
POST /realtime/connect
Body: { "connectionId": "socket_id_2" }

// الخادم الآن يعرف أن نفس المستخدم متصل من جهازين
```

---

#### **2. المستخدم يغير التطبيق بسرعة (App Switcher):**

```dart
// عند العودة للتطبيق:
// الـ socket قد يكون قد قطع

@override
void onResume() {
  // تحقق من الاتصال
  if (!pusher.isConnected()) {
    _reconnect();
  }
}

Future<void> _reconnect() async {
  await pusher.connect();
  await http.post(
    Uri.parse('$baseUrl/api/v1/realtime/connect'),
    headers: {'Authorization': 'Bearer $token'},
    body: jsonEncode({'connectionId': pusher.getSocketId()}),
  );
}
```

---

#### **3. الإنترنت اتقطع ورجعت:**

```dart
// Pusher تحاول الاتصال تلقائياً
// لكن قد تحتاج لإعادة تسجيل المحادثة النشطة

pusher.onConnectionStateChanged((change) {
  if (change.currentState == 'CONNECTED') {
    // إعادة تسجيل المحادثة النشطة
    if (activeConversationId != null) {
      _openConversation(activeConversationId!);
    }
  }
});
```

---

### **النصائح الذهبية:**

| النصيحة | التفاصيل |
|--------|---------|
| 🔒 **الأمان** | استخدم دائماً HTTPS وليس HTTP |
| 🔄 **الإعادة** | استخدم retry logic مع exponential backoff |
| 🧹 **التنظيف** | استدع disconnect() عند logout بشكل صريح |
| 📱 **الأجهزة المتعددة** | يدعم عدة sockets لنفس المستخدم |
| 🎯 **التركيز** | استدع active-conversation فقط عند المحادثة النشطة |
| ⏱️ **التوقيت** | انتظر response قبل الانتقال للخطوة التالية |

---

## 🔗 المراجع

- [REALTIME_FLUTTER_GUIDE.md](REALTIME_FLUTTER_GUIDE.md) - الدليل الشامل
- [Pusher Documentation](https://pusher.com/docs)
- ClinicHub API Documentation

---

**آخر تحديث:** مايو 2026  
**الحالة:** ✅ جاهز للاستخدام
