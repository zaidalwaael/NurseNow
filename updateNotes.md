# Nurse Services API Documentation

## Overview
This file documents the **service-related APIs** implemented for the nurse side in Sprint 2.

The current design supports:

- **Service Catalog** as a dropdown list source
- **Fixed duration** coming from the database
- **Price** entered by the nurse
- **CRUD operations** for the nurse's offered services

---

## Base Route
`/api/nurse`

---

## 1) Get Service Catalog

### Endpoint
`GET /api/nurse/service-catalog`

### Purpose
Returns the fixed list of nursing services used to populate the **dropdown list** in the Add Service / Edit Service screen.

### Response Example
```json
[
  {
    "serviceCatalogId": 1,
    "name": "IV Therapy",
    "defaultDurationInMinutes": 60
  },
  {
    "serviceCatalogId": 2,
    "name": "Wound Care and Dressing",
    "defaultDurationInMinutes": 60
  },
  {
    "serviceCatalogId": 3,
    "name": "Injection or Medication Administration",
    "defaultDurationInMinutes": 60
  },
  {
    "serviceCatalogId": 17,
    "name": "Short Care Shift (4 hours)",
    "defaultDurationInMinutes": 240
  }
]
```

### Flutter Usage
- Call this endpoint when opening the **Add Service** screen.
- Fill the dropdown with `name`.
- When a service is selected, display `defaultDurationInMinutes` as a fixed value.

---

## 2) Get My Services

### Endpoint
`GET /api/nurse/services`

### Purpose
Returns all services currently added by the logged-in nurse.

### Response Example
```json
[
  {
    "serviceId": 1,
    "serviceCatalogId": 1,
    "serviceName": "IV Therapy",
    "durationInMinutes": 60,
    "price": 25.0
  },
  {
    "serviceId": 2,
    "serviceCatalogId": 5,
    "serviceName": "Medication Management",
    "durationInMinutes": 60,
    "price": 18.5
  }
]
```

### Flutter Usage
- Use this endpoint to display the **Offered Services** section in the profile widget.

---

## 3) Add Service

### Endpoint
`POST /api/nurse/services`

### Purpose
Adds a new service for the logged-in nurse.

### Request Body
```json
{
  "serviceCatalogId": 1,
  "price": 25.0
}
```

### Notes
- `serviceCatalogId` comes from the dropdown list.
- `price` is entered by the nurse.
- Duration is **not sent** from Flutter because it is fixed in the database.

### Success Response Example
```json
"Service added successfully."
```

### Validation Logic
- The selected service must exist in `ServiceCatalog`.
- The nurse cannot add the same service twice.

### Possible Error Example
```json
"Invalid service."
```

or

```json
"This service has already been added."
```

---

## 4) Update Service

### Endpoint
`PUT /api/nurse/services/{id}`

### Purpose
Updates an existing service for the logged-in nurse.

### Request Body
```json
{
  "serviceCatalogId": 5,
  "price": 22.0
}
```

### Notes
- The nurse can change:
  - the selected service
  - the price
- Duration stays fixed based on the selected `ServiceCatalogId`.

### Success Response Example
```json
"Service updated successfully."
```

### Possible Error Example
```json
"Service not found."
```

or

```json
"Invalid service."
```

---

## 5) Delete Service

### Endpoint
`DELETE /api/nurse/services/{id}`

### Purpose
Deletes one of the nurse's services.

### Success Response Example
```json
"Service deleted successfully."
```

### Possible Error Example
```json
"Service not found."
```

---

## Returned JSON Shapes Summary

### A) Service Catalog Item
```json
{
  "serviceCatalogId": 1,
  "name": "IV Therapy",
  "defaultDurationInMinutes": 60
}
```

### B) Nurse Service Item
```json
{
  "serviceId": 1,
  "serviceCatalogId": 1,
  "serviceName": "IV Therapy",
  "durationInMinutes": 60,
  "price": 25.0
}
```

### C) Add / Update Request Body
```json
{
  "serviceCatalogId": 1,
  "price": 25.0
}
```

---

## Flutter Flow

### Add Service Screen
1. Call `GET /api/nurse/service-catalog`
2. Show service names in dropdown
3. When user selects a service:
   - show duration from `defaultDurationInMinutes`
4. Nurse enters price
5. Send `POST /api/nurse/services`

### Edit Service Screen
1. Load current service data from `GET /api/nurse/services`
2. Open selected service
3. Show dropdown selected value
4. Show fixed duration from catalog
5. Update price
6. Send `PUT /api/nurse/services/{id}`

### Services List in Profile
1. Call `GET /api/nurse/services`
2. Render each service with:
   - serviceName
   - durationInMinutes
   - price
3. Provide edit and delete actions

---

## Current Database Logic

### ServiceCatalog Table
Stores:
- service name
- default duration

### Services Table
Stores:
- nurse id
- selected service catalog id
- nurse price

This design ensures:
- consistent dropdown values
- fixed duration controlled by the database
- cleaner normalization
- easier filtering and booking later






/*****************************************/
update profile task 
# Nurse Update Profile API Documentation

## Overview
This document explains the **Nurse Update Profile** flow, including:

- the API used
- how image/file upload works
- what fields are sent
- what response JSON is returned

This endpoint is used when the nurse completes or updates the profile by sending:

- personal info
- professional details
- profile image
- certificate PDF
- national ID image

---

## Endpoint

### Method
`PUT`

### Route
`/api/nurse/update-profile`

### Authorization
This endpoint requires a valid **JWT token** for a user with the role:

`Nurse`

---

## Content Type

The request must be sent as:

`multipart/form-data`

This is required because the request contains both:
- text fields
- uploaded files

---

## What the endpoint updates

### Text fields
These fields are sent through `UpdateNurseProfileDto`:

- `phoneNumber`
- `address`
- `location`
- `bio`
- `licenseNumber`
- `specialization`
- `experienceYears`
- `nationalId`

### File fields
These are sent as `IFormFile` parameters:

- `profileImage`
- `certificate`
- `nationalIdImage`

---

## DTO Used

### `UpdateNurseProfileDto`

```csharp
public class UpdateNurseProfileDto
{
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Location { get; set; }
    public string? Bio { get; set; }

    public string? LicenseNumber { get; set; }
    public string? Specialization { get; set; }
    public int? ExperienceYears { get; set; }

    public string? NationalId { get; set; }
}
```

---

## Files Upload Logic

### 1) Upload folder
Uploaded files are saved inside:

`wwwroot/uploads`

If the folder does not exist, the API creates it automatically.

### 2) Unique file names
Each uploaded file gets a unique name using `Guid` to avoid name conflicts.

### 3) Stored in database
The system does **not** store files inside the database.

Instead, it stores only the relative path, for example:

- `uploads/abc123.jpg`
- `uploads/file1.pdf`
- `uploads/idimg77.png`

### 4) Allowed file types

#### Profile image
Allowed:
- `.jpg`
- `.jpeg`
- `.png`

#### Certificate
Allowed:
- `.pdf`

#### National ID image
Allowed:
- `.jpg`
- `.jpeg`
- `.png`

---

## Request Example

Because this endpoint uses `multipart/form-data`, the body is not sent as raw JSON.

### Form fields

#### Text fields
- `phoneNumber` = `0799999999`
- `address` = `Amman - Khalda`
- `location` = `Amman`
- `bio` = `Experienced home care nurse`
- `licenseNumber` = `RN-12345`
- `specialization` = `Critical Care`
- `experienceYears` = `5`
- `nationalId` = `123456789`

#### Files
- `profileImage` = image file
- `certificate` = pdf file
- `nationalIdImage` = image file

---

## Internal Flow

When the nurse sends the request, the system does the following:

1. Reads the logged-in nurse ID from the JWT token.
2. Retrieves the nurse profile from the database.
3. Updates the text fields in `NurseProfiles`.
4. Saves uploaded files in `wwwroot/uploads`.
5. Updates these database fields if files are uploaded:
   - `ProfileImagePath`
   - `CertificatePath`
   - `NationalIdImagePath`
6. Saves all changes in the database.

---

## Example Response JSON

### Success Response
```json
{
  "message": "Profile updated successfully.",
  "profileImagePath": "uploads/7d1b2c3a.jpg",
  "certificatePath": "uploads/9f8a7b6c.pdf",
  "nationalIdImagePath": "uploads/1122aabb.png"
}
```

---

## Possible Error Responses

### Profile not found
```json
"Profile not found."
```

### Invalid profile image type
```json
"Profile image must be JPG, JPEG, or PNG."
```

### Invalid certificate type
```json
"Certificate must be a PDF file."
```

### Invalid national ID image type
```json
"National ID image must be JPG, JPEG, or PNG."
```

---

## Related Database Fields

These fields are stored in `NurseProfiles`:

- `PhoneNumber`
- `Address`
- `Location`
- `Bio`
- `LicenseNumber`
- `Specialization`
- `ExperienceYears`
- `NationalId`
- `ProfileImagePath`
- `CertificatePath`
- `NationalIdImagePath`

---

## Important Note for Admin Approval

If the admin approval logic checks profile completeness, then the nurse should upload:

- certificate
- national ID image
- license number
- specialization
- experience years
- phone number
- address
- location

Otherwise the admin should not approve the nurse.

---

## Flutter Integration Notes

### Request type
Use:
- `MultipartRequest`

### Send:
- text fields as form fields
- images and pdf as files

### Recommended UI flow
1. Nurse opens Complete Profile screen
2. Fills in text data
3. Selects image/pdf files
4. Presses Save
5. Flutter sends `PUT /api/nurse/update-profile`
6. UI handles success response and updates local state

---

## Summary

The `update-profile` API is a **multipart/form-data** endpoint used to update the nurse profile and upload files.

It:
- updates nurse text fields
- saves uploaded files on the server
- stores only file paths in the database
- returns the saved paths in the response
