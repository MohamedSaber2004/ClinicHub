# Clinic Staff API — Frontend Integration Guide

## Base URL

```
https://your-domain.com/api/v1/admin/clinics/staff
```

All endpoints require authentication. Include the JWT token in the `Authorization` header:

```
Authorization: Bearer <token>
```

---

## Endpoints

### 1. Get Staff by ID

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/admin/clinics/staff/{id}` | Get a single staff member by ID |

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

#### Error Response — Staff Not Found (404)

```json
{
  "success": false,
  "errors": {},
  "data": null,
  "message": "Staff member not found",
  "statusCode": 404
}
```

#### Error Response — Validation Error (400)

```json
{
  "success": false,
  "errors": {
    "Id": ["'Id' must not be empty."]
  },
  "data": null,
  "message": "Validation failed",
  "statusCode": 400
}
```

---

### 2. Get All Staff (Paginated)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/admin/clinics/staff` | Get paginated list of staff for the current clinic |

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

---

### 3. Create Staff

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/admin/clinics/staff` | Create a new staff member |

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

---

### 4. Update Staff

| Method | Endpoint | Description |
|--------|----------|-------------|
| `PUT` | `/api/v1/admin/clinics/staff/{id}` | Update a staff member |

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

---

### 5. Delete Staff

| Method | Endpoint | Description |
|--------|----------|-------------|
| `DELETE` | `/api/v1/admin/clinics/staff/{id}` | Soft-delete a staff member |

#### Response (200 OK)

```json
{
  "success": true,
  "errors": {},
  "data": true,
  "message": "Success",
  "statusCode": 200
}
```

---

## StaffDto Model

| Field | Type | Description |
|-------|------|-------------|
| `id` | string (uuid) | Unique identifier |
| `fullName` | string | Staff member's full name |
| `email` | string | Email address |
| `phoneNumber` | string (nullable) | Phone number |
| `isActive` | boolean | Whether the staff member is active |
| `createdAt` | string (ISO 8601) | Creation timestamp |

---

## Authorization

These endpoints require the user to be authenticated with the `ClinicOwner` role and have an active subscription with the `ManageStaff` permission.

## Localization

The API supports Arabic and English via the `Accept-Language` header. Default is Arabic.

```
Accept-Language: en
```
