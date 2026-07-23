# ClinicHub - Get Active Specializations Frontend Integration Guide

This document is a dedicated integration guide designed for **frontend web developers** and **AI coding agents** working on client applications (React, Vue, Next.js, Angular, Mobile Web, Vanilla TS/JS). It details how to consume the anonymous **Get Active Specializations** endpoint, complete with request headers, HTTP parameters, full response schemas with all fields, TypeScript interfaces, and integration code snippets.

---

## 1. Endpoint Summary

| Property | Details |
| :--- | :--- |
| **HTTP Method** | `GET` |
| **Route Path** | `/api/v1/specializations/active` |
| **Authentication** | **Anonymous** (`[AllowAnonymous]`) - No authorization bearer token required. |
| **Access Level** | Public |
| **Response Format** | `application/json` (`ApiResponse<SpecializationLookupDto[]>`) |

---

## 2. HTTP Request Specification

### Request Headers

| Header Name | Type | Required | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Accept` | `string` | **Yes** | `application/json` | Expected media response format. |
| `Accept-Language` | `string` | Optional | `ar` | Language preference (`ar` or `en`) for localized messages. |
| `Authorization` | `string` | **No** | N/A | Anonymous endpoint. Do NOT send bearer token unless desired. |

### Request Query Parameters
*No query parameters are required for this request.*

### Request Body
*None (HTTP GET requests do not contain a request body).*

```http
GET /api/v1/specializations/active HTTP/1.1
Host: api.clinichub.com
Accept: application/json
Accept-Language: ar
```

---

## 3. Full HTTP Response Specification (All Fields)

### Success Response (`200 OK`)

The endpoint returns a standard unified API envelope (`ApiResponse<T>`) containing an array of active specialization objects (`SpecializationLookupDto[]`).

#### Complete JSON Response Payload:

```json
{
  "success": true,
  "errors": {},
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "arName": "طب الأسنان",
      "name": "Dentistry",
      "isFamous": true
    },
    {
      "id": "b1a2c3d4-5717-4562-b3fc-2c963f66afa7",
      "arName": "الجلدية والتناسلية",
      "name": "Dermatology",
      "isFamous": true
    },
    {
      "id": "c2b3a4d5-5717-4562-b3fc-2c963f66afa8",
      "arName": "طب الأطفال",
      "name": "Pediatrics",
      "isFamous": true
    },
    {
      "id": "d3c4b5a6-5717-4562-b3fc-2c963f66afa9",
      "arName": "أمراض القلب والأوعية الدموية",
      "name": "Cardiology",
      "isFamous": true
    }
  ],
  "message": null,
  "statusCode": 200
}
```

#### Detailed Response Field Definitions

##### 1. Envelope Root Fields (`ApiResponse<T>`)

| Field Name | Type | Always Present | Description |
| :--- | :--- | :--- | :--- |
| `success` | `boolean` | **Yes** | `true` when the operation completes successfully. |
| `errors` | `object` (`Record<string, string[]>`) | **Yes** | Key-value map of error messages. Empty object `{}` on success. |
| `data` | `array<object>` | **Yes** | Array of `SpecializationLookupDto` items. |
| `message` | `string \| null` | **Yes** | Optional response message string (`null` for standard lookup queries). |
| `statusCode` | `integer` | **Yes** | Standard HTTP Status Code (`200`). |

##### 2. Specialization Element Fields (`SpecializationLookupDto`)

| Field Name | Data Type | Required / Nullable | Example Value | Field Description |
| :--- | :--- | :--- | :--- | :--- |
| `id` | `string` (UUID v4) | Required (Non-null) | `"3fa85f64-5717-4562-b3fc-2c963f66afa6"` | Unique identifier of the specialization. Use this ID when submitting forms (e.g., `specializationId` in clinic registration). |
| `arName` | `string` | Required (Non-null) | `"طب الأسنان"` | Arabic localized name of the medical specialization. |
| `name` | `string` | Required (Non-null) | `"Dentistry"` | Primary / English name of the medical specialization. |
| `isFamous` | `boolean` | Required (Non-null) | `true` | Indicates if this specialization is featured / popular. |

---

### Error Responses

#### Internal Server Error (`500 Internal Server Error`)
Returned if an unhandled error occurs on the server.

```json
{
  "success": false,
  "errors": {},
  "data": null,
  "message": "An unexpected error occurred while processing your request.",
  "statusCode": 500
}
```

---

## 4. TypeScript Definitions for Frontend & AI Agents

Frontend applications and AI coding agents can copy and paste these TypeScript interfaces:

```typescript
/**
 * Single Specialization Lookup Item
 */
export interface SpecializationLookupDto {
  /** Unique ID (UUID string) */
  id: string;
  /** Arabic localized name */
  arName: string;
  /** Primary / English name */
  name: string;
  /** Featured / Famous specialization indicator */
  isFamous: boolean;
}

/**
 * Standard Unified API Envelope Wrapper
 */
export interface ApiResponse<TData> {
  /** Success status indicator */
  success: boolean;
  /** Error map (empty object when successful) */
  errors: Record<string, string[]>;
  /** Payload data */
  data: TData | null;
  /** System message */
  message: string | null;
  /** HTTP Status code */
  statusCode: number;
}

/**
 * Specific Response Type for Active Specializations Endpoint
 */
export type GetActiveSpecializationsResponse = ApiResponse<SpecializationLookupDto[]>;
```

---

## 5. Ready-to-Use Code Snippets

### A. Fetch API (TypeScript / JavaScript)

```typescript
import { GetActiveSpecializationsResponse, SpecializationLookupDto } from './types';

const API_BASE_URL = 'https://api.clinichub.com';

/**
 * Fetches active specializations for form dropdowns or filters.
 * @param lang Preferred response language ('ar' | 'en')
 */
export async function fetchActiveSpecializations(
  lang: 'ar' | 'en' = 'ar'
): Promise<SpecializationLookupDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/v1/specializations/active`, {
    method: 'GET',
    headers: {
      'Accept': 'application/json',
      'Accept-Language': lang
    }
  });

  if (!response.ok) {
    throw new Error(`HTTP Error: ${response.status} ${response.statusText}`);
  }

  const result: GetActiveSpecializationsResponse = await response.json();

  if (!result.success || !result.data) {
    throw new Error(result.message || 'Failed to retrieve active specializations.');
  }

  return result.data;
}
```

### B. Axios Example (TypeScript)

```typescript
import axios from 'axios';
import { GetActiveSpecializationsResponse, SpecializationLookupDto } from './types';

const api = axios.create({
  baseURL: 'https://api.clinichub.com/api/v1'
});

export async function getActiveSpecializations(lang: 'ar' | 'en' = 'ar'): Promise<SpecializationLookupDto[]> {
  const response = await api.get<GetActiveSpecializationsResponse>('/specializations/active', {
    headers: {
      'Accept-Language': lang
    }
  });

  if (response.data.success && response.data.data) {
    return response.data.data;
  }

  throw new Error(response.data.message || 'Error fetching specializations');
}
```

---

## 6. How to Use in UI Components

1. **Populating Dropdowns**: Use `id` as the `<option value={spec.id}>` and `arName` or `name` as the displayed label based on active UI locale.
2. **Clinic Registration**: Send the selected `id` as `specializationId` in the registration request payload to `/api/v1/clinics/register`.
3. **Caching**: Because active specializations change infrequently, frontend clients are recommended to cache this list in memory or local storage for the duration of the session.
