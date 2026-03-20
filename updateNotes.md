# Nurse Details Section Documentation

## Overview
This documentation explains the backend work completed for the **Patient Nurse Details section**.

This section is shown when the patient clicks on a nurse card from the **Browse Nurses** screen.

The goal of this endpoint is to return all data needed to render the nurse details/profile screen.

---

## Endpoint

### Method
`GET`

### Route
`/api/patient/nurses/{nurseId}`

### Authorization
Requires a valid JWT token for a user with role:

`Patient`

---

## Purpose
This endpoint returns:

- nurse basic profile data
- nurse image
- headline
- experience years
- location
- address
- current availability label
- qualification/certificate file URL
- list of services offered

---

## Returned Fields

The endpoint returns the following fields:

- `nurseId`
- `fullName`
- `profileImageUrl`
- `headline`
- `rating`
- `reviewsCount`
- `experienceYears`
- `location`
- `address`
- `availabilityLabel`
- `certificateUrl`
- `servicesOffered`

---

## Response Example

```json
{
  "nurseId": "123",
  "fullName": "Sarah Hassan",
  "profileImageUrl": "https://localhost:5001/uploads/profile.jpg",
  "headline": "IV Therapy & Wound Care",
  "rating": 0.0,
  "reviewsCount": 0,
  "experienceYears": 8,
  "location": "Amman",
  "address": "Abdali",
  "availabilityLabel": "Available Today",
  "certificateUrl": "https://localhost:5001/uploads/certificate.pdf",
  "servicesOffered": [
    "IV Therapy",
    "Wound Care",
    "Medication Management"
  ]
}
```

---

## Data Sources Used

The endpoint depends on the following tables:

- `AspNetUsers`
- `NurseProfiles`
- `Services`
- `ServiceCatalog`
- `WeeklyAvailabilities`
- `AvailabilityOverrides`

---

## Backend Logic

### 1) Approved nurses only
The endpoint returns nurse details only if:

- the nurse exists
- `VerificationStatus == "Approved"`

If not, the API returns:

```json
"Nurse not found."
```

---

### 2) Nurse basic info
The following fields are read from:

#### `AspNetUsers`
- `FullName`

#### `NurseProfiles`
- `ExperienceYears`
- `Location`
- `Address`
- `ProfileImagePath`
- `CertificatePath`
- `Specialization`

---

### 3) Profile image URL
The stored path in the database is converted into a full URL.

### Example stored path
```text
uploads/profile.jpg
```

### Returned URL
```text
https://localhost:5001/uploads/profile.jpg
```

This is generated using:
- `Request.Scheme`
- `Request.Host`
- `ProfileImagePath`

---

### 4) Certificate URL
The nurse qualification/certification is not returned as a text list.

Instead, the API returns:
- `certificateUrl`

This URL points to the uploaded certificate file (image or PDF).

### Example
```text
https://localhost:5001/uploads/certificate.pdf
```

This allows the frontend to:
- open the PDF
- display the image
- or open it externally

---

### 5) Services Offered
The endpoint retrieves all services added by the nurse from:

- `Services`
- `ServiceCatalog`

Only the service names are returned in the `servicesOffered` array.

### Example
```json
"servicesOffered": [
  "IV Therapy",
  "Wound Care",
  "Medication Management"
]
```

---

### 6) Headline logic
The API builds the nurse headline using services.

#### Rule:
- if the nurse has 2 or more services:
  - use the first two services joined by `&`
- if the nurse has only 1 service:
  - use that service
- otherwise:
  - fallback to `Specialization`
  - if specialization is null → fallback to `"Nurse"`

### Example
```text
IV Therapy & Wound Care
```

---

### 7) Availability label logic
The API generates a simple availability label using:

- `WeeklyAvailabilities`
- `AvailabilityOverrides`

### Current rules
#### Available Today
If:
- there is an override for today and it is not blocked
- OR the nurse has active weekly availability for today and today is not blocked

#### Available This Week
If:
- the nurse has any active weekly availability this week

#### Unavailable
If:
- no active weekly availability exists

### Current returned values
- `Available Today`
- `Available This Week`
- `Unavailable`

---

### 8) Rating and reviews count
Currently, the rating system is not implemented yet.

So the API temporarily returns:

```json
"rating": 0.0,
"reviewsCount": 0
```

Later, when review/rating logic is added:
- `rating` will be calculated as average rating
- `reviewsCount` will be the number of reviews

---

## Flutter Usage

This endpoint is used when the patient opens the nurse details screen.

### Flow
1. Patient clicks a nurse card
2. Frontend receives `nurseId`
3. Frontend calls:

```http
GET /api/patient/nurses/{nurseId}
```

4. The response is used to fill:
- header
- about section
- availability label
- certificate button/link
- services offered list

---

## Notes

### Price
The price field was intentionally removed from this section.

The nurse details page currently focuses on:
- profile information
- availability
- services
- certificate

not price display.

### Certificate display
Qualifications & Certifications in the UI are handled as:
- a link or button to open the uploaded certificate

not as a textual qualifications list.

---

## Status

✔ Nurse Details endpoint implemented  
✔ Full profile image URL implemented  
✔ Certificate URL implemented  
✔ Headline generated from services  
✔ Availability label logic implemented  
✔ Services offered list implemented  

⏳ Pending:
- real rating system
- reviews count
- booking integration
*********************************************************************************



# Book Service Request Flow Documentation

## Overview
This document explains everything implemented in the backend **after the patient clicks "Book Service Request"** from the nurse details page.

This flow covers:
- selecting a nurse service
- loading available dates
- loading available time slots
- reviewing the request
- submitting the booking request
- handling the success response for the confirmation screen

---

# 1) Flow Summary

After the patient clicks **Book Service Request**, the system follows this sequence:

1. Load the nurse services
2. Patient selects one service
3. Load available dates for that nurse
4. Patient selects a date
5. Load available time slots for the selected service and date
6. Patient selects a time slot
7. Patient enters service address
8. Patient enters optional additional notes
9. Patient clicks **Review and Confirm**
10. Flutter navigates to the review page locally
11. Patient clicks **Submit Request**
12. Backend creates the booking request
13. Backend returns booking summary
14. Flutter opens the success screen

---

# 2) Service Request Screen APIs

## A) Get Nurse Services

### Endpoint
`GET /api/patient/nurses/{nurseId}/services`

### Purpose
Returns only the services offered by the selected nurse.

### Response Example
```json
[
  {
    "serviceId": 1,
    "serviceCatalogId": 1,
    "serviceName": "IV Therapy",
    "durationInMinutes": 60,
    "price": 50.0
  },
  {
    "serviceId": 2,
    "serviceCatalogId": 2,
    "serviceName": "Wound Care & Dressing",
    "durationInMinutes": 60,
    "price": 30.0
  }
]
```

### Notes
- `serviceId` is the nurse-specific service record
- `serviceCatalogId` is the service type from the catalog
- `durationInMinutes` comes from `ServiceCatalog`
- `price` comes from the nurse's configured service price

---

## B) Get Available Dates

### Endpoint
`GET /api/patient/nurses/{nurseId}/available-dates?daysAhead=14`

### Purpose
Returns available dates for the selected nurse in the coming days.

### Logic Used
The endpoint checks:
- `WeeklyAvailabilities`
- `AvailabilityOverrides`
- blocked dates

### Response Example
```json
[
  "2026-03-11",
  "2026-03-12",
  "2026-03-14",
  "2026-03-16"
]
```

### Notes
- blocked days are excluded
- override dates with valid start/end times are included
- weekly availability days are included if active

---

## C) Get Available Time Slots

### Endpoint
`GET /api/patient/nurses/{nurseId}/available-slots?serviceId=1&date=2026-03-11`

### Purpose
Returns time slots for the selected nurse, service, and date.

### Logic Used
The endpoint:
1. loads the selected service
2. gets the service duration
3. determines working hours for that date using:
   - `AvailabilityOverrides`
   - or `WeeklyAvailabilities`
4. splits the working hours into slots using the service duration

### Response Example
```json
[
  "09:00",
  "10:00",
  "11:00",
  "12:00",
  "14:00",
  "15:00"
]
```

### Notes
- blocked days return an empty list
- if no availability exists, an empty list is returned
- currently this endpoint does not yet subtract already booked slots until booking integration becomes more advanced

---

# 3) Review and Confirm Page

## Important Decision
The **Review and Confirm** button does **not** require a new backend endpoint.

### Reason
This page is only a confirmation screen that shows the same data already selected in the previous page.

### Flutter Behavior
When the patient clicks **Review and Confirm**:
- Flutter navigates to the next screen
- passes the selected values locally:
  - nurse
  - service
  - duration
  - date
  - time
  - address
  - notes
  - price

---

# 4) Booking Model

The project already had a `Booking` model, so instead of creating a new one, it was updated to support the service request flow.

## Updated Booking Model Fields
```csharp
public class Booking
{
    public int BookingId { get; set; }

    public string PatientId { get; set; }
    public string NurseId { get; set; }

    public int ServiceId { get; set; }

    public DateTime BookingDate { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public string ServiceAddress { get; set; }
    public string? AdditionalNotes { get; set; }

    public string Status { get; set; }
    public string PaymentStatus { get; set; }

    public ApplicationUser Patient { get; set; }
    public ApplicationUser Nurse { get; set; }

    public Service Service { get; set; }

    public Payment Payment { get; set; }
    public ICollection<Complaint> Complaints { get; set; }
}
```

### Newly Added Fields
- `ServiceId`
- `StartTime`
- `EndTime`
- `ServiceAddress`
- `AdditionalNotes`

---

# 5) Create Booking Request DTO

## DTO
`CreateBookingRequestDto`

```csharp
public class CreateBookingRequestDto
{
    public string NurseId { get; set; }
    public int ServiceId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public string ServiceAddress { get; set; }
    public string? AdditionalNotes { get; set; }
}
```

---

# 6) Create Booking Request API

## Endpoint
`POST /api/patient/bookings`

## Purpose
Creates a new booking request after the patient confirms the request.

---

## Request Example
```json
{
  "nurseId": "123",
  "serviceId": 1,
  "date": "2026-11-03",
  "startTime": "10:00:00",
  "serviceAddress": "Jabal Amman",
  "additionalNotes": "Please call before arrival."
}
```

---

## Validation Logic

The backend checks the following before creating the booking:

### 1) Patient authentication
- the patient must be logged in

### 2) Nurse validity
- the selected nurse must exist
- the nurse must be approved

### 3) Service validity
- the selected service must belong to the selected nurse

### 4) Service address
- service address must not be empty

### 5) Day blocked check
- if the selected date is blocked, the booking is rejected

### 6) Working hours check
- if there is an override, it uses override working hours
- otherwise, it uses weekly availability

### 7) Time range validation
- selected `StartTime` must be inside working hours
- calculated `EndTime` must also be inside working hours

### 8) Slot conflict check
- if the exact same slot is already booked and not rejected, the booking is rejected

---

## EndTime Calculation
The backend calculates `EndTime` automatically using:

- selected `StartTime`
- service duration from `ServiceCatalog`

---

## Initial Booking Status
A new booking is created with:

`Pending`

---

# 7) Create Booking Response

The booking creation response was enhanced so the frontend can directly show the success screen.

## Response Example
```json
{
  "message": "Booking request submitted successfully.",
  "bookingId": 5,
  "status": "Pending",
  "summary": {
    "nurseName": "Sarah Hassan",
    "serviceName": "IV Therapy",
    "durationInMinutes": 60,
    "date": "2026-03-11",
    "time": "15:00",
    "totalPrice": 50.0
  }
}
```

---

# 8) Success Screen

## Important Decision
The **Request Submitted** screen does **not** need its own backend endpoint.

### Reason
The success screen can be filled directly from the response of:

`POST /api/patient/bookings`

### Flutter Behavior
After `Submit Request`:
1. backend creates the booking
2. backend returns `summary`
3. Flutter navigates to the success screen
4. Flutter displays:
   - nurse name
   - service
   - duration
   - date
   - time
   - total price

---

# 9) Backend Tables Used in this Flow

The booking flow currently depends on:

- `AspNetUsers`
- `NurseProfiles`
- `Services`
- `ServiceCatalog`
- `WeeklyAvailabilities`
- `AvailabilityOverrides`
- `Bookings`

---

# 10) Backend Status

## Completed
- Get nurse services
- Get available dates
- Get available time slots
- Review & Confirm page decision
- Booking model update
- Create booking request DTO
- Create booking request API
- Success response summary

## Pending
- Nurse view requests
- Nurse accept / reject request
- Upcoming appointments
- Patient booking history
- payment flow integration
- booked slot subtraction refinement

---

# 11) Final Flow

## Patient Side
1. Open nurse details
2. Click **Book Service Request**
3. Load nurse services
4. Select service
5. Load available dates
6. Select date
7. Load available time slots
8. Select time slot
9. Enter address and notes
10. Click **Review and Confirm**
11. Flutter opens review page locally
12. Click **Submit Request**
13. Backend creates booking
14. Flutter opens success screen using returned summary
