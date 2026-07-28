# Get Doctor & Update Doctor — Frontend Integration

---

## 1. Get Doctor By ID

Fetches a doctor's full profile including user data and weekly schedule.

### Endpoint

```
GET /api/v{version}/doctors/{id:guid}
```

**Auth:** `SuperAdmin`, `ClinicOwner`, any authenticated role  
**Route constant:** `ApiRoutes.Doctors.GetById`

### Response (200 OK)

```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userName": "Ahmed Mohamed",
    "userEmail": "ahmed.doctor@clinic.com",
    "userPhoneNumber": "+201234567890",
    "profilePictureUrl": "doctor_avatar_abc123.jpg",
    "clinicId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "clinicName": "ClinicHub Medical Center",
    "specializationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "specializationName": "قلبية",
    "bio": "Experienced cardiologist with 10+ years",
    "yearsOfExperience": 10,
    "isActive": true,
    "createdAt": "2026-07-28T21:00:00Z",
    "availabilities": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "dayOfWeek": 1,
        "startTime": "09:00:00",
        "endTime": "17:00:00",
        "slotDurationMinutes": 30
      }
    ]
  },
  "message": "Ok",
  "statusCode": 200
}
```

### Response fields

| Field | Type | Source |
|---|---|---|
| `id` | Guid | Doctor.Id |
| `userId` | Guid | Doctor.UserId → ApplicationUser.Id |
| `userName` | string | ApplicationUser.FullName |
| `userEmail` | string | ApplicationUser.Email |
| `userPhoneNumber` | string? | ApplicationUser.PhoneNumber |
| `profilePictureUrl` | string? | ApplicationUser.ProfilePictureUrl (filename, not URL) |
| `clinicId` | Guid | Doctor.ClinicId |
| `clinicName` | string? | Clinic.Name |
| `specializationId` | Guid | Doctor.SpecializationId |
| `specializationName` | string? | Specialization.ArName (Arabic) |
| `bio` | string | Doctor.Bio |
| `yearsOfExperience` | int | Doctor.YearsOfExperience |
| `isActive` | bool | Doctor.IsActive |
| `createdAt` | datetime | Doctor.CreatedAt |
| `availabilities` | array | Non-deleted DoctorAvailability entries |

#### Availability item

| Field | Type | Notes |
|---|---|---|
| `id` | Guid | Availability record ID (for reference only) |
| `dayOfWeek` | int | 0=Sunday … 6=Saturday |
| `startTime` | string | `HH:mm:ss` (24h) |
| `endTime` | string | `HH:mm:ss` (24h) |
| `slotDurationMinutes` | int | Appointment slot length |

---

## 2. Update Doctor

Updates user info, doctor profile, and/or availability in a single transaction.

### Endpoint

```
PUT /api/v{version}/doctors/{id:guid}
```

**Auth:** `SuperAdmin`, `ClinicOwner`  
**Plan permission:** `ManageDoctors`

> ClinicOwner can only update doctors belonging to their own clinic.

### Request Body — All Fields Optional

```json
{
  "fullName": "Ahmed Mohamed Updated",
  "email": "ahmed.new@clinic.com",
  "phoneNumber": "+201234567891",
  "birthDate": "1990-05-15",
  "gender": 1,
  "doctorImage": "doctor_new_photo.jpg",
  "bio": "Updated bio text",
  "yearsOfExperience": 12,
  "isActive": true,
  "availabilities": [
    {
      "dayOfWeek": 1,
      "startTime": "09:00:00",
      "endTime": "17:00:00",
      "slotDurationMinutes": 30
    }
  ]
}
```

### Field reference

| Field | Type | Behaviour when `null`/omitted |
|---|---|---|
| `fullName` | string? | Not changed |
| `email` | string? | Not changed (validated for uniqueness if provided) |
| `phoneNumber` | string? | Not changed (validated for uniqueness if provided) |
| `birthDate` | string? (date) | Not changed |
| `gender` | int? | Not changed (1=Male, 2=Female) |
| `doctorImage` | string? | Not changed (filename only, not URL) |
| `bio` | string? | Not changed |
| `yearsOfExperience` | int? | Not changed (must be >= 0) |
| `isActive` | bool? | Not changed (activate/deactivate doctor) |
| `availabilities` | array | **Only replaced when `availabilities.length > 0`** |

### How availability update works

| Scenario | Behaviour |
|---|---|
| `availabilities` omitted or `null` | Existing schedule stays unchanged |
| `availabilities: []` (empty array) | Existing schedule stays unchanged |
| `availabilities: [{...}, ...]` (has items) | ALL existing availability records are **deleted** and replaced with the new ones |

### Transaction guarantee

User data (FullName, Email, Phone, BirthDate, Gender, ProfilePicture), doctor data (Bio, YearsOfExperience, IsActive), and availability are updated in a **single database transaction**. Either all succeed or all fail.

### Response (200 OK)

Same shape as Get Doctor response — returns the updated doctor with all fields.

---

## 3. Frontend Integration

### UI — Edit Doctor Form

```
┌───────────────────────────────────────┐
│  Edit Doctor                          │
│                                       │
│  ── Account Info ──                   │
│                                       │
│  Full Name    [Ahmed Mohamed______]   │
│  Email        [ahmed@clinic.com___]   │
│  Phone        [+201234567890_____]   │
│  Birth Date   [____ date picker ___]  │
│  Gender       [Male ▾]               │
│  Profile Pic  [Upload / Change]      │
│                                       │
│  ── Professional Info ──              │
│                                       │
│  Bio          [textarea pre-filled]   │
│  Experience   [12] years              │
│  Active       [toggle: yes]           │
│                                       │
│  ── Working Hours ──                  │
│  ┌───────────────────────────────┐    │
│  │ Day: [Mon ▾] From [09:00]    │    │
│  │ To:  [17:00] Slot [30] min   │    │
│  │ [Remove]                     │    │
│  ├───────────────────────────────┤    │
│  │ [+ Add Day]                  │    │
│  └───────────────────────────────┘    │
│                                       │
│  [Cancel]              [Save]         │
└───────────────────────────────────────┘
```

### Step 1 — Fetch current data

```dart
import 'package:dio/dio.dart';

final dio = Dio(BaseOptions(
  baseUrl: 'https://api.clinichub.com',
  headers: { 'Authorization': 'Bearer $accessToken' },
));

// Fetch doctor
final getRes = await dio.get('/api/v1/doctors/$doctorId');
final doctor = getRes.data['data'];

// Populate form
fullNameCtrl.text = doctor['userName'];
emailCtrl.text = doctor['userEmail'];
phoneCtrl.text = doctor['userPhoneNumber'];
birthDateCtrl.text = doctor['birthDate'];       // format as needed
genderValue = doctor['gender'];                  // 1 or 2
profilePicUrl = doctor['profilePictureUrl'];
bioCtrl.text = doctor['bio'];
experienceCtrl.text = doctor['yearsOfExperience'].toString();
isActive = doctor['isActive'];

workingHours = (doctor['availabilities'] as List).map((a) => WorkingHour(
  dayOfWeek: a['dayOfWeek'],
  startTime: a['startTime'].substring(0, 5),    // "09:00"
  endTime: a['endTime'].substring(0, 5),        // "17:00"
  slotDurationMinutes: a['slotDurationMinutes'],
)).toList();
```

### Step 2 — Save changes (send only changed fields)

```dart
Map<String, dynamic> body = {};

if (changed.fullName) body['fullName'] = fullNameCtrl.text;
if (changed.email) body['email'] = emailCtrl.text;
if (changed.phone) body['phoneNumber'] = phoneCtrl.text;
if (changed.birthDate) body['birthDate'] = birthDateCtrl.text;
if (changed.gender) body['gender'] = genderValue;
if (changed.doctorImage) body['doctorImage'] = uploadedFileName;
if (changed.bio) body['bio'] = bioCtrl.text;
if (changed.experience) body['yearsOfExperience'] = int.parse(experienceCtrl.text);
if (changed.isActive != null) body['isActive'] = isActive;
if (changed.availabilities) {
  body['availabilities'] = workingHours.map((wh) => {
    'dayOfWeek': wh.dayOfWeek,
    'startTime': '${wh.startTime}:00',
    'endTime': '${wh.endTime}:00',
    'slotDurationMinutes': wh.slotDurationMinutes,
  }).toList();
}

final putRes = await dio.put(
  '/api/v1/doctors/$doctorId',
  data: body,
);
```

### Step 3 — Handle response

```dart
if (putRes.statusCode == 200) {
  final updatedDoctor = putRes.data['data'];
  // Update local state with the fresh data
  doctorState = updatedDoctor;
  showSnackBar('Doctor updated successfully');
}
```

### Error handling

```dart
try {
  final res = await dio.put('/api/v1/doctors/$doctorId', data: body);
  // success
} on DioException catch (e) {
  if (e.response?.statusCode == 400) {
    final errors = e.response?.data['errors'];
    // errors is a Map<String, List<String>>
    // e.g. { "Email": ["Email already exists"] }
    showValidationErrors(errors);
  } else if (e.response?.statusCode == 404) {
    showError('Doctor not found');
  } else {
    showError('An unexpected error occurred');
  }
}
```

---

## 4. Doctor List (Get by Clinic)

Fetches all doctors in a clinic with pagination.

### Endpoint

```
GET /api/v{version}/admin/clinics/{clinicId:guid}/doctors?PageNumber=1&PageSize=20
```

**Auth:** `SuperAdmin`, `ClinicOwner`  
**Route constant:** `ApiRoutes.Doctors.GetAllByClinic`

### Response (200 OK)

```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "...",
      "userName": "Ahmed Mohamed",
      "userEmail": "ahmed@clinic.com",
      "profilePictureUrl": "...",
      "specializationId": "...",
      "specializationName": "قلبية",
      "bio": "...",
      "yearsOfExperience": 10,
      "isActive": true,
      "createdAt": "...",
      "availabilities": []
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 5,
  "totalPages": 1
}
```

---

## 5. Quick Notes for Flutter

| Concept | Detail |
|---|---|
| `dayOfWeek` | Integer 0–6 (Sunday=0), match your day picker index |
| Time format | Always `HH:mm:ss` in API; strip `:00` for display, append for send |
| `gender` | 1=Male, 2=Female |
| `doctorImage` | Filename string only; upload separately, then send the returned filename |
| Empty vs omitted | `"bio": null` → bio unchanged; omit field entirely for same effect |
| Availability replace | Only when `availabilities.length > 0`; empty array or null = no change |
| Transaction | All changes (user + doctor + availability) are atomic |
| Response shape | Always wrapped in `{ data: {...}, message: "...", statusCode: XXX }` |
| Error shape | `{ data: null, errors: { "FieldName": ["Error msg"] }, message: "...", statusCode: 400 }` |
