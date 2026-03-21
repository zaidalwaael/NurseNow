# Nurse Service Requests Section Documentation

## Overview
This documentation explains the backend work completed for the **Nurse Service Requests section**.

This section allows the nurse to:
- view all booking requests
- filter requests by status
- open full request details
- accept pending requests
- decline pending requests

Supported statuses:
- `Pending`
- `Accepted`
- `Rejected`

---

## Endpoints Summary

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/nurse/requests` | Get all nurse requests |
| GET | `/api/nurse/requests?status=Pending` | Get pending requests only |
| GET | `/api/nurse/requests?status=Accepted` | Get accepted requests only |
| GET | `/api/nurse/requests?status=Rejected` | Get rejected requests only |
| GET | `/api/nurse/requests/{bookingId}` | Get full details of one request |
| PUT | `/api/nurse/requests/{bookingId}/accept` | Accept a pending request |
| PUT | `/api/nurse/requests/{bookingId}/decline` | Decline a pending request |

---

# 1) Get Nurse Requests

## Endpoint
`GET /api/nurse/requests`

## Optional Query Parameter
- `status`

Examples:
```http
GET /api/nurse/requests
GET /api/nurse/requests?status=Pending
GET /api/nurse/requests?status=Accepted
GET /api/nurse/requests?status=Rejected
```

## Purpose
Returns the booking requests assigned to the logged-in nurse.

## Returned Fields
Each item includes:
- `bookingId`
- `patientName`
- `patientPhone`
- `serviceName`
- `date`
- `time`
- `location`
- `durationInMinutes`
- `totalPrice`
- `notes`
- `status`

## Response Example
```json
[
  {
    "bookingId": 5,
    "patientName": "Ali Mohammed",
    "patientPhone": "0799999999",
    "serviceName": "IV Therapy",
    "date": "2026-11-03",
    "time": "10:00",
    "location": "Jabal Amman",
    "durationInMinutes": 60,
    "totalPrice": 50.0,
    "notes": "Please call before arrival.",
    "status": "Pending"
  }
]
```

---

# 2) Get Request Details

## Endpoint
`GET /api/nurse/requests/{bookingId}`

## Purpose
Returns the full details of one request for the logged-in nurse.

## Returned Fields
- `bookingId`
- `patientName`
- `contact`
- `serviceType`
- `date`
- `time`
- `location`
- `durationInMinutes`
- `payment`
- `additionalNotes`
- `status`

## Response Example
```json
{
  "bookingId": 5,
  "patientName": "Ali Mohammed",
  "contact": "0799999999",
  "serviceType": "IV Therapy",
  "date": "2026-11-03",
  "time": "10:00",
  "location": "Jabal Amman",
  "durationInMinutes": 60,
  "payment": 50.0,
  "additionalNotes": "Please call before arrival.",
  "status": "Pending"
}
```

## Error Example
```json
"Request not found."
```

---

# 3) Accept Request

## Endpoint
`PUT /api/nurse/requests/{bookingId}/accept`

## Purpose
Changes the request status from `Pending` to `Accepted`.

## Validation
- the request must belong to the logged-in nurse
- only `Pending` requests can be accepted

## Response Example
```json
{
  "message": "Request accepted successfully.",
  "status": "Accepted"
}
```

## Error Example
```json
"Only pending requests can be accepted."
```

---

# 4) Decline Request

## Endpoint
`PUT /api/nurse/requests/{bookingId}/decline`

## Purpose
Changes the request status from `Pending` to `Rejected`.

## Validation
- the request must belong to the logged-in nurse
- only `Pending` requests can be declined

## Response Example
```json
{
  "message": "Request declined successfully.",
  "status": "Rejected"
}
```

## Error Example
```json
"Only pending requests can be declined."
```

---

# Booking Status Flow

The current request flow supports:

- `Pending`
- `Accepted`
- `Rejected`

### Initial status
When the patient creates a booking request:
- the booking is created with status:
`Pending`

### Nurse actions
- Accept → `Accepted`
- Decline → `Rejected`

---

# Backend Tables Used

These APIs currently depend on:

- `Bookings`
- `AspNetUsers`
- `Services`
- `ServiceCatalog`

---

# Required Booking Fields

The `Booking` model used by this section includes:

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
    public string Status { get; set; } = "Pending";

    public ApplicationUser Patient { get; set; }
    public ApplicationUser Nurse { get; set; }
    public Service Service { get; set; }
}
```

---

# Flutter Usage

## Tabs
The nurse UI can use these tabs:

- All
- Pending
- Accepted
- Rejected

Each tab calls:
- `GET /api/nurse/requests`
- optionally with `status`

## Full Details
When the nurse clicks **View Full Details**:
- Flutter calls:
`GET /api/nurse/requests/{bookingId}`

## Accept / Decline
When the nurse clicks:
- **Accept Request** → call accept endpoint
- **Decline** → call decline endpoint

---

# Notes

## Rejected
Rejected requests are supported and can be loaded using:

```http
GET /api/nurse/requests?status=Rejected
```

## Payment
The request details currently return:
- `payment = booking.Service.Price`

This represents the service price for the request.

## Notifications
Notifications to the patient after accept/decline are not implemented yet.

---

# Status

✔ Requests list implemented  
✔ Status filter implemented  
✔ Request details implemented  
✔ Accept request implemented  
✔ Decline request implemented  
✔ Rejected tab supported  

⏳ Pending:
- patient notifications after status change
- upcoming appointments integration
- payment workflow integration
