# Nurse Browse Section Documentation

## Overview
This documentation explains the backend work completed for the **Patient Browse Nurses section**.

The goal of this section is to allow the patient to:
- browse approved nurses
- search by nurse name, specialty, or location
- filter by service
- filter by location (governorate)
- load more nurses using pagination / infinite scroll

---

## Endpoint

### Method
`GET`

### Route
`/api/patient/nurses/browse`

### Authorization
Requires a valid JWT token for a user with role:

`Patient`

---

## Query Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `search` | string | No | Search by nurse name, specialty, or location |
| `serviceCatalogId` | int | No | Filter by selected service |
| `location` | string | No | Filter by governorate |
| `pageNumber` | int | No | Page number for pagination |
| `pageSize` | int | No | Number of items per page |

---

## Search Logic

The `search` field matches any of:

- `FullName`
- `Specialization`
- `Location`

This means the patient can type:
- nurse name
- specialty
- location

and get matching results.

---

## Filtering Logic

### 1) Filter by Service
If `serviceCatalogId` is sent:
- only nurses who provide this service are returned

### 2) Filter by Location
If `location` is sent:
- only nurses in that governorate are returned

### 3) Filter by Service + Location
Both filters can be used together.

---

## Sorting Logic

### Default Sorting
If no location filter is applied:
- results are sorted by **rating descending**
- currently, since rating is not implemented yet, the temporary sorting uses:
  - `ExperienceYears DESC`

### Sorting when Location Filter Exists
If `location` is applied:
- results are sorted by **Address ascending**
- this helps organize nurses alphabetically by area inside the governorate

---

## Pagination Logic

The endpoint supports pagination using:
- `pageNumber`
- `pageSize`

This is useful for infinite scroll in Flutter.

### Example
```http
GET /api/patient/nurses/browse?pageNumber=1&pageSize=10
```

### Example with filters
```http
GET /api/patient/nurses/browse?search=critical&serviceCatalogId=1&location=Amman&pageNumber=1&pageSize=10
```

---

## Returned Data

Each nurse item includes:

- `nurseId`
- `fullName`
- `specialization`
- `location`
- `address`
- `experienceYears`
- `profileImageUrl`
- `rating`
- `reviewsCount`
- `price`
- `availabilityLabel`

---

## Price Logic

### If `serviceCatalogId` is provided
The API returns:
- the nurse price for that specific service

### If no service filter is provided
The API returns:
- the minimum service price for that nurse

---

## Availability Label Logic

Currently, the API uses a simple availability rule:

- if the nurse has active weekly availability:
  - `Available This Week`
- otherwise:
  - `Unavailable`

This is a temporary version until booking integration is implemented.

---

## Profile Image URL

The API returns a **full image URL**, not just the stored path.

### Example
```json
"profileImageUrl": "https://localhost:5001/uploads/abc.jpg"
```

This is generated using:

- `Request.Scheme`
- `Request.Host`
- stored `ProfileImagePath`

---

## Request Example

```http
GET /api/patient/nurses/browse?search=critical&serviceCatalogId=1&location=Amman&pageNumber=1&pageSize=10
```

---

## Response Example

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 3,
  "totalPages": 1,
  "hasNextPage": false,
  "items": [
    {
      "nurseId": "123",
      "fullName": "Sarah Hassan",
      "specialization": "Critical Care",
      "location": "Amman",
      "address": "Abdali",
      "experienceYears": 8,
      "profileImageUrl": "https://localhost:5001/uploads/abc.jpg",
      "rating": 0.0,
      "reviewsCount": 0,
      "price": 25.0,
      "availabilityLabel": "Available This Week"
    }
  ]
}
```

---

## Current Notes

### Rating
The default sorting requirement is:
- by rating descending

However, since the rating system is not implemented yet, the current temporary fallback is:
- sort by `ExperienceYears DESC`

### Reviews Count
Currently returned as:
- `0`

until review/rating system is implemented.

---

## Backend Dependencies Used

The endpoint depends on:
- `AspNetUsers`
- `NurseProfiles`
- `Services`
- `ServiceCatalog`
- `WeeklyAvailabilities`

---

## Flutter Usage

The patient home page or browse screen can use this endpoint to:

1. load the first page of nurses
2. search by text
3. filter by service
4. filter by location
5. load more on scroll using pagination

---

## Status

✔ Browse Nurses endpoint implemented  
✔ Search logic implemented  
✔ Service filter implemented  
✔ Location filter implemented  
✔ Pagination implemented  
✔ Full profile image URL implemented  

⏳ Pending:
- real rating system
- reviews count
- booking-aware availability
