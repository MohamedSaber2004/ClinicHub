# Clinic Dashboard API — Frontend Integration Guide

## Base URL

```
https://your-domain.com/api/v1
```

All endpoints require authentication. Include the JWT token in the `Authorization` header:

```
Authorization: Bearer <token>
```

---

## 1. Staff Management

### Base Route: `/admin/clinics/staff`

### 1.1 Get All Staff (Paginated)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/admin/clinics/staff` | Get paginated list of staff for the current clinic |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `PageNumber` | int | No | `1` | Page number |
| `PageSize` | int | No | `20` | Items per page |
| `SearchTerm` | string | No | — | Search by name or email |
| `IsActive` | bool | No | — | Filter by active status |

#### Request

```
GET /api/v1/admin/clinics/staff?PageNumber=1&PageSize=10&SearchTerm=ahmed&IsActive=true
Authorization: Bearer <token>
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "Ahmed Mohamed",
        "email": "ahmed@clinic.com",
        "phoneNumber": "+201234567890",
        "isActive": true,
        "createdAt": "2026-07-26T10:30:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "message": "Success",
  "statusCode": 200
}
```

### 1.2 Get Staff by ID

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/admin/clinics/staff/{id}` | Get a single staff member by ID |

#### Request

```
GET /api/v1/admin/clinics/staff/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer <token>
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "Ahmed Mohamed",
    "email": "ahmed@clinic.com",
    "phoneNumber": "+201234567890",
    "isActive": true,
    "createdAt": "2026-07-26T10:30:00Z"
  },
  "message": "Success",
  "statusCode": 200
}
```

#### Error — Not Found (404)

```json
{
  "success": false,
  "errors": {},
  "data": null,
  "message": "Staff member not found",
  "statusCode": 404
}
```

### 1.3 Create Staff

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/admin/clinics/staff` | Create a new staff member |

#### Request Body

```json
{
  "fullName": "Ahmed Mohamed",
  "email": "ahmed@clinic.com",
  "phoneNumber": "+201234567890",
  "password": "P@ssw0rd!"
}
```

#### Success Response (201 Created)

```json
{
  "success": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "Ahmed Mohamed",
    "email": "ahmed@clinic.com",
    "phoneNumber": "+201234567890",
    "isActive": true,
    "createdAt": "2026-07-26T10:30:00Z"
  },
  "message": "Created",
  "statusCode": 201
}
```

### 1.4 Update Staff

| Method | Endpoint | Description |
|--------|----------|-------------|
| `PUT` | `/admin/clinics/staff/{id}` | Update a staff member |

#### Request Body

```json
{
  "fullName": "Ahmed Updated",
  "phoneNumber": "+201098765432",
  "isActive": false
}
```

All fields are optional — only provided fields are updated.

#### Success Response (200 OK)

```json
{
  "success": true,
  "errors": {},
  "data": true,
  "message": "Success",
  "statusCode": 200
}
```

### 1.5 Delete Staff

| Method | Endpoint | Description |
|--------|----------|-------------|
| `DELETE` | `/admin/clinics/staff/{id}` | Soft-delete a staff member |

#### Success Response (200 OK)

```json
{
  "success": true,
  "errors": {},
  "data": true,
  "message": "Success",
  "statusCode": 200
}
```

### 1.6 Change Staff Password

| Method | Endpoint | Description |
|--------|----------|-------------|
| `PUT` | `/admin/clinics/staff/{id}/change-password` | Change a staff member's password |

#### Request Body

```json
{
  "newPassword": "NewStr0ng!Pass",
  "confirmPassword": "NewStr0ng!Pass"
}
```

**Password Requirements:**
- Minimum length as configured (default: 8 characters)
- Must contain at least one uppercase letter
- Must contain at least one digit
- Must NOT match the current password

#### Success Response (200 OK)

```json
{
  "success": true,
  "errors": {},
  "data": null,
  "message": "Success",
  "statusCode": 200
}
```

#### Error — Same as Current Password (400)

```json
{
  "success": false,
  "errors": {
    "NewPassword": ["New password cannot be the same as the current password"]
  },
  "data": null,
  "message": "Validation failed",
  "statusCode": 400
}
```

#### Error — Weak Password (400)

```json
{
  "success": false,
  "errors": {
    "NewPassword": ["For your security, please choose a stronger password."]
  },
  "data": null,
  "message": "Validation failed",
  "statusCode": 400
}
```

#### Error — Password Mismatch (400)

```json
{
  "success": false,
  "errors": {
    "ConfirmPassword": ["Passwords do not match"]
  },
  "data": null,
  "message": "Validation failed",
  "statusCode": 400
}
```

---

## 2. Doctor Management

### Base Route: `/admin/clinics/doctors`

### 2.1 Get All Doctors by Clinic

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/admin/clinics/{clinicId}/doctors` | Get paginated list of doctors for a clinic |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `PageNumber` | int | No | `1` | Page number |
| `PageSize` | int | No | `20` | Items per page |
| `SearchTerm` | string | No | — | Search by name |
| `SpecializationId` | guid | No | — | Filter by specialization |
| `IsActive` | bool | No | — | Filter by active status |

### 2.2 Get Doctor by ID

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/doctors/{id}` | Get a single doctor by ID |

### 2.3 Create Doctor (for ClinicOwner)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/admin/clinics/doctors` | Create a new doctor for the current clinic |

### 2.4 Update Doctor

| Method | Endpoint | Description |
|--------|----------|-------------|
| `PUT` | `/doctors/{id}` | Update a doctor's details |

### 2.5 Delete Doctor

| Method | Endpoint | Description |
|--------|----------|-------------|
| `DELETE` | `/doctors/{id}` | Soft-delete a doctor |

### 2.6 Change Doctor Password

| Method | Endpoint | Description |
|--------|----------|-------------|
| `PUT` | `/admin/clinics/doctors/{id}/change-password` | Change a doctor's password |

#### Request Body

```json
{
  "newPassword": "NewStr0ng!Pass",
  "confirmPassword": "NewStr0ng!Pass"
}
```

**Password Requirements:**
- Minimum length as configured (default: 8 characters)
- Must contain at least one uppercase letter
- Must contain at least one digit
- Must NOT match the current password

#### Success Response (200 OK)

```json
{
  "success": true,
  "errors": {},
  "data": null,
  "message": "Success",
  "statusCode": 200
}
```

#### Error — User Not Found (404)

```json
{
  "success": false,
  "errors": {},
  "data": null,
  "message": "User not found",
  "statusCode": 404
}
```

---

## 3. Change Password — Full API Reference

### Endpoints

| Role | Endpoint | Required Permission |
|------|----------|---------------------|
| Staff | `PUT /api/v1/admin/clinics/staff/{id}/change-password` | `ManageStaff` |
| Doctor | `PUT /api/v1/admin/clinics/doctors/{id}/change-password` | `ManageDoctors` |

### Request

```
PUT /api/v1/admin/clinics/staff/3fa85f64-5717-4562-b3fc-2c963f66afa6/change-password
Authorization: Bearer <token>
Content-Type: application/json

{
  "newPassword": "NewStr0ng!Pass",
  "confirmPassword": "NewStr0ng!Pass"
}
```

### Validation Rules

| Field | Rule |
|-------|------|
| `userId` | Required (inferred from route `{id}`) |
| `newPassword` | Required, min length (default 8), must contain uppercase + digit, must NOT match current password |
| `confirmPassword` | Required, must match `newPassword` |

### Error Codes

| Status Code | Description |
|-------------|-------------|
| `200` | Password changed successfully |
| `400` | Validation failed (weak password, same as old, mismatch) |
| `404` | User not found, not in your clinic, or not a staff/doctor |
| `401` | Unauthenticated |
| `403` | Forbidden (not a ClinicOwner or missing subscription permission) |

---

## 4. Common Models

### StaffDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | string (uuid) | Unique identifier |
| `fullName` | string | Full name |
| `email` | string | Email address |
| `phoneNumber` | string (nullable) | Phone number |
| `isActive` | boolean | Active status |
| `createdAt` | string (ISO 8601) | Creation timestamp |

### ApiResponse Envelope

| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Whether the request succeeded |
| `errors` | object | Field-level validation errors |
| `data` | T | Response payload (nullable) |
| `message` | string (nullable) | Human-readable message |
| `statusCode` | int | HTTP status code |

---

## 5. Authorization

- **Staff Management** — Requires `ClinicOwner` role + active subscription with `ManageStaff` permission
- **Doctor Management** — Requires `ClinicOwner` role + active subscription with `ManageDoctors` permission

## 6. Localization

The API supports Arabic and English via the `Accept-Language` header. Default is Arabic.

```
Accept-Language: en
```
