# Patient Appointments Section Documentation

## Overview
This documentation explains the backend work completed for the **Patient Appointments section**.

This section allows the patient to:
- view all appointments
- filter appointments into:
  - upcoming
  - past
- open appointment details
- cancel eligible appointments

---

# 1) Appointment Tabs Logic

The UI contains two main tabs:

## Upcoming
This tab includes appointments with statuses:
- `Pending`
- `Accepted`
- `Active`

## Past
This tab includes appointments with statuses:
- `Cancelled`
- `Rejected`
- `Completed`

> The grouping is based on **status**, not only on date.

---

# 2) Get Patient Appointments API

## Endpoint
`GET /api/patient/appointments?tab=upcoming`

## Purpose
Returns the patient's appointments list for either:
- upcoming
- past

## Query Parameter
- `tab`

Allowed values:
- `upcoming`
- `past`

### Examples
```http
GET /api/patient/appointments?tab=upcoming
GET /api/patient/appointments?tab=past
```

---

## Returned Fields
Each appointment item includes:
- `bookingId`
- `nurseName`
- `profileImageUrl`
- `serviceName`
- `date`
- `time`
- `address`
- `totalPrice`
- `status`

---

## Response Example
```json
[
  {
    "bookingId": 5,
    "nurseName": "Sarah Hassan",
    "profileImageUrl": "https://localhost:5001/uploads/profile.jpg",
    "serviceName": "IV Therapy",
    "date": "2026-01-12",
    "time": "14:00",
    "address": "123 Main St, Abdali, Amman",
    "totalPrice": 50.0,
    "status": "Accepted"
  },
  {
    "bookingId": 6,
    "nurseName": "Layla Ahmed",
    "profileImageUrl": null,
    "serviceName": "Wound Care",
    "date": "2026-01-13",
    "time": "10:00",
    "address": "456 King St, Sweifieh, Amman",
    "totalPrice": 30.0,
    "status": "Pending"
  }
]
```

---

# 3) Appointment Details API

## Endpoint
`GET /api/patient/appointments/{bookingId}`

## Purpose
Returns full details of a specific appointment for the logged-in patient.

## Returned Fields
- `bookingId`
- `nurseName`
- `profileImageUrl`
- `phoneNumber`
- `serviceName`
- `date`
- `time`
- `address`
- `durationInMinutes`
- `totalPrice`
- `additionalNotes`
- `status`

---

## Response Example
```json
{
  "bookingId": 5,
  "nurseName": "Sarah Hassan",
  "profileImageUrl": "https://localhost:5001/uploads/profile.jpg",
  "phoneNumber": "0799999999",
  "serviceName": "IV Therapy",
  "date": "2026-11-03",
  "time": "10:00",
  "address": "Jabal Amman",
  "durationInMinutes": 60,
  "totalPrice": 50.0,
  "additionalNotes": "Please call before arrival.",
  "status": "Accepted"
}
```

## Error Example
```json
"Appointment not found."
```

---

# 4) Cancel Appointment API

## Endpoint
`PUT /api/patient/appointments/{bookingId}/cancel`

## Purpose
Allows the patient to cancel an appointment.

## Cancellation Rules
The patient can cancel only if the appointment status is:
- `Pending`
- `Accepted`

The patient cannot cancel if the status is:
- `Rejected`
- `Cancelled`
- `Completed`
- `Active`

---

## Response Example
```json
{
  "message": "Appointment cancelled successfully.",
  "status": "Cancelled"
}
```

## Error Example
```json
"Only pending or accepted appointments can be cancelled."
```

---

# 5) Booking Statuses Used

The patient appointments section currently uses these statuses:

- `Pending`
- `Accepted`
- `Active`
- `Cancelled`
- `Rejected`
- `Completed`

---

# 6) Data Sources Used

The endpoints depend on:
- `Bookings`
- `AspNetUsers`
- `NurseProfiles`
- `Services`
- `ServiceCatalog`

---

# 7) Backend Logic Details

## A) List Appointments
The list endpoint:
- gets all bookings for the logged-in patient
- groups them by tab using status
- joins nurse and service data
- returns summary cards for the UI

## B) Appointment Details
The details endpoint:
- checks that the appointment belongs to the logged-in patient
- loads nurse and service info
- loads nurse phone number from `NurseProfiles`
- returns a full appointment details object

## C) Cancel Appointment
The cancel endpoint:
- checks that the appointment belongs to the logged-in patient
- validates the current status
- changes status to `Cancelled`
- saves changes

---

# 8) Flutter Usage

## Upcoming tab
```http
GET /api/patient/appointments?tab=upcoming
```

## Past tab
```http
GET /api/patient/appointments?tab=past
```

## Appointment card click
```http
GET /api/patient/appointments/{bookingId}
```

## Cancel button
```http
PUT /api/patient/appointments/{bookingId}/cancel
```

---

# 9) Important Notes

## Profile Image URL
The profile image is returned as a full URL if available.

## Phone Number
The nurse phone number is returned in the details endpoint only.

## Cancelled appointments
Once an appointment is cancelled:
- it will no longer appear in `upcoming`
- it will appear in `past`

---

# 10) Status

✔ Upcoming appointments endpoint implemented  
✔ Past appointments endpoint implemented  
✔ Appointment details endpoint implemented  
✔ Cancel appointment endpoint implemented  
✔ Nurse phone number included in details  

⏳ Pending:
- appointment notifications
- automatic active/completed status transitions
- payment integration

*************************************************************************************************
# Nurse Appointment Details Section Documentation

## Overview
This documentation explains the backend work completed for the **Nurse Appointment Details section**.

This section allows the nurse to:
- open a specific appointment
- view full appointment details
- see the patient phone number for contact
- mark an appointment as completed
- cancel an appointment

---

# 1) Appointment Details API

## Endpoint
`GET /api/nurse/appointments/{bookingId}`

## Purpose
Returns full details of one appointment for the logged-in nurse.

## Returned Fields
- `bookingId`
- `patientName`
- `phoneNumber`
- `serviceName`
- `date`
- `time`
- `address`
- `totalPrice`
- `additionalNotes`
- `status`

## Response Example
```json
{
  "bookingId": 12,
  "patientName": "Ali Mohammed",
  "phoneNumber": "0799999999",
  "serviceName": "IV Therapy",
  "date": "2026-03-11",
  "time": "15:00",
  "address": "Jabal Amman",
  "totalPrice": 50.0,
  "additionalNotes": "Please call before arrival.",
  "status": "Accepted"
}
```

## Error Example
```json
"Appointment not found."
```

---

# 2) Mark Appointment as Completed

## Endpoint
`PUT /api/nurse/appointments/{bookingId}/complete`

## Purpose
Allows the nurse to mark an appointment as completed.

## Allowed Statuses
This action is allowed only if the current status is:
- `Accepted`
- `Active`

## Response Example
```json
{
  "message": "Appointment marked as completed successfully.",
  "status": "Completed"
}
```

## Error Example
```json
"Only accepted or active appointments can be marked as completed."
```

---

# 3) Cancel Appointment by Nurse

## Endpoint
`PUT /api/nurse/appointments/{bookingId}/cancel`

## Purpose
Allows the nurse to cancel an appointment.

## Allowed Statuses
This action is allowed only if the current status is:
- `Pending`
- `Accepted`

## Response Example
```json
{
  "message": "Appointment cancelled successfully.",
  "status": "Cancelled"
}
```

## Error Example
```json
"Only pending or accepted appointments can be cancelled."
```

---

# 4) Data Sources Used

These APIs depend on:
- `Bookings`
- `AspNetUsers`
- `Services`
- `ServiceCatalog`

---

# 5) Backend Logic

## Appointment Details
The API:
- checks that the appointment belongs to the logged-in nurse
- loads patient data
- loads service data
- returns patient phone number for contact

## Complete Appointment
The API:
- checks ownership
- validates current status
- updates status to `Completed`

## Cancel Appointment
The API:
- checks ownership
- validates current status
- updates status to `Cancelled`

---

# 6) Booking Statuses Used

These actions depend on the following statuses:

- `Pending`
- `Accepted`
- `Active`
- `Cancelled`
- `Completed`

### UI mapping suggestion
If you want to show:
- `Confirmed` in the UI

You can map:
- `Accepted` → `Confirmed`

in Flutter.

---

# 7) Flutter Usage

## Open appointment details
```http
GET /api/nurse/appointments/{bookingId}
```

## Mark as completed
```http
PUT /api/nurse/appointments/{bookingId}/complete
```

## Cancel appointment
```http
PUT /api/nurse/appointments/{bookingId}/cancel
```

---

# 8) UI Button Logic

## If status = Accepted
Show:
- Mark as Completed
- Cancel Appointment
- Contact Patient

## If status = Active
Show:
- Mark as Completed
- Contact Patient

## If status = Completed
Show:
- Contact Patient only

## If status = Cancelled
Show:
- status only

---

# 9) Status

✔ Nurse appointment details endpoint implemented  
✔ Patient phone number included  
✔ Complete appointment endpoint implemented  
✔ Cancel appointment endpoint implemented  

⏳ Pending:
- notifications after completion/cancellation
- payment flow integration
- automatic status transitions

