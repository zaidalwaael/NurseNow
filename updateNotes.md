# Availability Module Documentation

## Overview
The Availability Module allows nurses to manage their working schedule using:
- Weekly recurring availability (default schedule)
- Day-specific overrides
- Blocking specific days
- Managing time slots

---

## Data Models

### WeeklyAvailability
{
  "id": 1,
  "nurseId": "user-id",
  "dayOfWeek": 1,
  "startTime": "09:00",
  "endTime": "17:00",
  "isActive": true
}

### AvailabilityOverride
{
  "id": 10,
  "nurseId": "user-id",
  "date": "2026-06-15",
  "isBlocked": false,
  "startTime": "12:00",
  "endTime": "18:00"
}

---

## API Endpoints

### 1. Add Weekly Availability
POST /api/nurse/availability/weekly

Request:
{
  "dayOfWeek": 1,
  "startTime": "09:00",
  "endTime": "17:00"
}

Response:
{
  "message": "Availability added successfully"
}

---

### 2. Get Weekly Availability
GET /api/nurse/availability/weekly

Response:
[
  {
    "dayOfWeek": 1,
    "startTime": "09:00",
    "endTime": "17:00",
    "isActive": true
  }
]

---

### 3. Delete Weekly Slot
DELETE /api/nurse/availability/weekly/{id}

Response:
{
  "message": "Slot deleted successfully"
}

---

### 4. Toggle Slot
PUT /api/nurse/availability/weekly/toggle/{id}

Response:
{
  "message": "Slot status updated"
}

---

### 5. Get Day Details
GET /api/nurse/availability/day?date=2026-06-15

Response:
{
  "date": "2026-06-15",
  "isBlocked": false,
  "timeSlots": [
    {
      "startTime": "09:00",
      "endTime": "17:00"
    }
  ],
  "hasOverride": false,
  "bookedAppointments": []
}

---

### 6. Override Day
POST /api/nurse/availability/day/override

Request:
{
  "date": "2026-06-15",
  "startTime": "12:00",
  "endTime": "18:00"
}

Response:
{
  "message": "Day overridden successfully"
}

---

### 7. Block Day
POST /api/nurse/availability/day/block

Request:
{
  "date": "2026-06-15"
}

Response:
{
  "message": "Day blocked successfully"
}

---

### 8. Unblock Day
DELETE /api/nurse/availability/day/unblock?date=2026-06-15

Response:
{
  "message": "Day unblocked successfully"
}

---

## Status
✔ Backend Completed
⏳ Booking Integration Pending
