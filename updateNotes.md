# Admin Dashboard APIs Documentation

## Overview
This document explains the backend APIs implemented for the **Admin Dashboard**.

The dashboard currently includes:
- summary cards
- request status distribution chart
- weekly activity chart
- recent activity list

---

# 1) Dashboard Summary API

## Endpoint
```http
GET /api/admin/dashboard-summary
```

## Purpose
Returns the top summary cards data for the admin dashboard.

## Returned Data
- `totalPatients`
- `totalNurses`
- `pendingVerifications`
- `todaysRequests`

## Response Example
```json
{
  "totalPatients": 2847,
  "totalNurses": 456,
  "pendingVerifications": 23,
  "todaysRequests": 87
}
```

## Logic
- `totalPatients` is calculated from users with role type `Patient`
- `totalNurses` is calculated from approved nurse profiles
- `pendingVerifications` is calculated from nurse profiles with status `Pending`
- `todaysRequests` is calculated from bookings with today's booking date

---

# 2) Request Status Distribution API

## Endpoint
```http
GET /api/admin/request-status-distribution
```

## Purpose
Returns the number and percentage of bookings grouped by status.

## Example Statuses
- Pending
- Accepted
- Completed
- Cancelled
- Rejected

## Response Example
```json
[
  {
    "status": "Completed",
    "count": 57,
    "percentage": 57.0
  },
  {
    "status": "Accepted",
    "count": 21,
    "percentage": 21.0
  },
  {
    "status": "Pending",
    "count": 17,
    "percentage": 17.0
  },
  {
    "status": "Cancelled",
    "count": 5,
    "percentage": 5.0
  }
]
```

## Logic
- bookings are grouped by `Status`
- each group returns:
  - count
  - percentage relative to total bookings

---

# 3) Weekly Activity API

## Endpoint
```http
GET /api/admin/weekly-activity
```

## Purpose
Returns booking activity for the last 7 days.

## Returned Data
For each day:
- `day`
- `date`
- `requests`
- `completed`

## Response Example
```json
[
  {
    "day": "Mon",
    "date": "2026-04-01",
    "requests": 65,
    "completed": 52
  },
  {
    "day": "Tue",
    "date": "2026-04-02",
    "requests": 78,
    "completed": 68
  }
]
```

## Logic
- `requests` = total bookings for that day
- `completed` = bookings for that day with status `Completed`

## Note
This implementation uses `BookingDate` as the activity date source.

---

# 4) Recent Activity Log Model

## File
`Models/AdminActivityLog.cs`

## Structure
```csharp
namespace NurseNow.Models
{
    public class AdminActivityLog
    {
        public int AdminActivityLogId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public string ActivityType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

## Purpose
Stores recent admin-related activity that can be displayed in the dashboard.

---

# 5) ApplicationDbContext Update

## File
`Data/ApplicationDbContext.cs`

## DbSet
```csharp
public DbSet<AdminActivityLog> AdminActivityLogs { get; set; }
```

## Note
No relationship configuration is required for the current version because the model is independent.

---

# 6) Recent Activity API

## Endpoint
```http
GET /api/admin/recent-activity
```

## Optional Query Parameter
```http
?limit=5
```

## Purpose
Returns the most recent activity items for the admin dashboard.

## Response Example
```json
[
  {
    "activityId": 1,
    "title": "Nurse verification approved",
    "description": "Sarah Hassan verification was updated to Approved.",
    "activityType": "Verification",
    "createdAt": "2026-04-01T10:30:00Z"
  },
  {
    "activityId": 2,
    "title": "New service request submitted",
    "description": "A new booking request was submitted by patient ID abc123.",
    "activityType": "Booking",
    "createdAt": "2026-04-01T10:10:00Z"
  },
  {
    "activityId": 3,
    "title": "New nurse registration",
    "description": "Layla Ahmed registered as a nurse.",
    "activityType": "Registration",
    "createdAt": "2026-04-01T09:50:00Z"
  }
]
```

## Logic
- recent activity items are sorted by `CreatedAt` descending
- `limit` controls the number of returned records
- default limit = 5

---

# 7) Where Recent Activity Logs Are Created

## A) AuthController
When a new nurse registers:
- activity title: `New nurse registration`
- activity type: `Registration`

## B) AdminController
When admin approves or rejects nurse verification:
- activity title:
  - `Nurse verification approved`
  - `Nurse verification rejected`
- activity type: `Verification`

## C) PatientController
When a patient submits a new booking request:
- activity title: `New service request submitted`
- activity type: `Booking`

---

# 8) Migration

After adding `AdminActivityLog`:

```powershell
Add-Migration AddAdminActivityLog
Update-Database
```

---

# 9) Full Admin Dashboard Backend Coverage

The admin dashboard now has backend support for:

## Cards
```http
GET /api/admin/dashboard-summary
```

## Pie Chart
```http
GET /api/admin/request-status-distribution
```

## Line Chart
```http
GET /api/admin/weekly-activity
```

## Recent Activity
```http
GET /api/admin/recent-activity
```

---

# 10) Notes

## Recent Activity Scope
The first version of recent activity focuses on:
- nurse registration
- nurse verification updates
- new booking requests

This is enough to support the current dashboard design.

## Future Enhancements
Later, more activity types can be added, such as:
- complaint resolved
- pricing updates
- admin management changes
- service updates
- payment events

---

# 11) Final Result

The admin dashboard backend now supports:
- key summary statistics
- status analytics
- weekly booking activity
- recent activity feed

This makes the dashboard ready for frontend integration and real data display.


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

# Nurse Verification APIs Documentation

## Overview
This document explains the backend APIs implemented for the **Nurse Verification** section in the Admin Website.

The goal of this section is to allow the admin to:
- view all nurse accounts
- filter nurses by verification status
- search nurses by name or email
- view full nurse details
- approve or reject nurse verification
- trigger notifications and activity logs

---

# 1) Verification Status

The nurse verification process uses:

- `Pending`
- `Approved`
- `Rejected`

## Meaning

### Pending
The nurse has registered but has not been reviewed yet.

### Approved
The nurse has been verified and can start receiving bookings.

### Rejected
The nurse was rejected and cannot operate as a nurse.

---

# 2) Get All Nurses API

## Endpoint
```http
GET /api/admin/nurses
```

## Purpose
Returns all nurses for the verification table.

## Supported Query Parameters

### search
Search by:
- full name
- email

### status
Filter by:
- `Pending`
- `Approved`
- `Rejected`
- `All`

---

# 3) Example Requests

## Get all nurses
```http
GET /api/admin/nurses
```

## Filter pending nurses
```http
GET /api/admin/nurses?status=Pending
```

## Search by name
```http
GET /api/admin/nurses?search=sarah
```

## Search + filter
```http
GET /api/admin/nurses?search=sarah&status=Approved
```

---

# 4) Response Example

```json
[
  {
    "nurseId": "user-id-1",
    "name": "Sarah Hassan",
    "email": "sarah@email.com",
    "phone": "0791234567",
    "registrationDate": "2026-03-01T00:00:00",
    "status": "Pending"
  }
]
```

---

# 5) Get Nurse Details API

## Endpoint
```http
GET /api/admin/nurse-details/{userId}
```

## Purpose
Returns full nurse profile details for review.

---

# 6) Verify Nurse API

## Endpoint
```http
PUT /api/admin/verify-nurse/{userId}
```

## Request Body

```json
{
  "status": "Approved"
}
```

or

```json
{
  "status": "Rejected"
}
```

---

# 7) Validation Logic

Before approving a nurse, the system checks that the profile is complete.

---

# 8) Notifications

When verification status changes:

- Approved â Account Approved
- Rejected â Account Rejected

---

# 9) Activity Log

Each verification action creates an admin activity log.

---

# 10) Final Result

The Nurse Verification backend now supports:
- listing nurses
- searching
- filtering
- viewing details
- approving/rejecting
- notifications
- activity logging

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

# Users Management APIs Documentation

## Overview
This document explains the backend APIs and model updates implemented for the **Users Management** section in the Admin Website.

The goal of this section is to allow the admin to:
- view users by role
- search users by name or email
- filter users by account status
- suspend user accounts
- reactivate suspended user accounts

The Users Management section is based on:
- `RoleType`
- `AccountStatus`

---

# 1) Important Concept

## RoleType
Used to distinguish the user type:
- `Patient`
- `Nurse`

## AccountStatus
Used to control whether the account is active or suspended:
- `Active`
- `Suspended`

### Note
`AccountStatus` is different from nurse `VerificationStatus`.

#### VerificationStatus
Used only for nurse verification workflow:
- `Pending`
- `Approved`
- `Rejected`

#### AccountStatus
Used for the account itself:
- `Active`
- `Suspended`

---

# 2) ApplicationUser Update

## File
`Models/ApplicationUser.cs`

The following fields were added:

```csharp
public string AccountStatus { get; set; } = "Active";

public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
```

## Purpose
- `AccountStatus` stores whether the account is active or suspended
- `CreatedAt` stores the account creation date for join date display and sorting

---

# 3) Migration

After updating `ApplicationUser`:

```powershell
Add-Migration AddAccountStatus
Update-Database
```

---

# 4) Get Users API

## Endpoint
```http
GET /api/admin/users
```

## Purpose
Returns users for the Users Management table.

## Supported Query Parameters

### role
Filter by role:
- `Patient`
- `Nurse`
- `All`

### search
Search by:
- full name
- email

### status
Filter by account status:
- `Active`
- `Suspended`
- `All`

---

# 5) Example Requests

## Get all patients
```http
GET /api/admin/users?role=Patient
```

## Get all nurses
```http
GET /api/admin/users?role=Nurse
```

## Get active patients
```http
GET /api/admin/users?role=Patient&status=Active
```

## Get suspended nurses
```http
GET /api/admin/users?role=Nurse&status=Suspended
```

## Search by name
```http
GET /api/admin/users?search=john
```

## Search + role + status
```http
GET /api/admin/users?role=Patient&status=Active&search=john
```

---

# 6) Get Users Response Example

```json
[
  {
    "id": "user-id-1",
    "name": "John Doe",
    "email": "john.doe@email.com",
    "phone": "0791112222",
    "joinDate": "2026-01-15T00:00:00",
    "status": "Active",
    "role": "Patient"
  },
  {
    "id": "user-id-2",
    "name": "Jane Smith",
    "email": "jane.smith@email.com",
    "phone": "0792223333",
    "joinDate": "2026-01-20T00:00:00",
    "status": "Suspended",
    "role": "Patient"
  }
]
```

## Returned Fields
- `id`
- `name`
- `email`
- `phone`
- `joinDate`
- `status`
- `role`

---

# 7) Update User Status API

## Endpoint
```http
PUT /api/admin/users/{userId}/status
```

## Purpose
Allows the admin to:
- suspend a user account
- activate a suspended account

---

# 8) Request Body Example

## Suspend
```json
{
  "status": "Suspended"
}
```

## Activate
```json
{
  "status": "Active"
}
```

---

# 9) Response Example

```json
{
  "message": "User status updated to Suspended"
}
```

---

# 10) DTO

## File
`DTOs/UpdateUserStatusDto.cs`

```csharp
namespace NurseNow.DTOs
{
    public class UpdateUserStatusDto
    {
        public string Status { get; set; }
    }
}
```

---

# 11) Login Protection Update

## File
`Controllers/AuthController.cs`

The login logic should check `AccountStatus`.

## Added Logic
```csharp
if (user.AccountStatus == "Suspended")
    return Unauthorized("Your account is suspended");
```

## Result
Suspended users cannot log in.

---

# 12) UI Mapping

The Users Management screen uses:

## Tabs
- Patients
- Nurses

These map to:
- `role=Patient`
- `role=Nurse`

## Search
Maps to:
- `search`

## Filter
Maps to:
- `status=Active`
- `status=Suspended`

## Actions
- View
- Suspend
- Activate

---

# 13) Business Logic Summary

## Active
User account is allowed to log in and use the system normally.

## Suspended
User account is blocked and cannot log in.

## Important Difference
A nurse may be:
- `VerificationStatus = Approved`
- but still `AccountStatus = Suspended`

That means:
- the nurse was approved before
- but later suspended by admin

---

# 14) What Was Implemented

## Completed
- `AccountStatus` field in `ApplicationUser`
- `CreatedAt` field in `ApplicationUser`
- migration for user status
- users list API with role filter
- users list API with status filter
- users list API with search
- update user status API
- DTO for status update
- login protection for suspended accounts

---

# 15) Final Result

The Users Management backend now supports:
- viewing users by role
- filtering by account status
- searching by name or email
- suspending accounts
- reactivating accounts
- preventing suspended users from logging in

This makes the Users Management section ready for frontend integration.

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

Service Requests Management (Admin) — API Documentation

## Overview
This module enables Admins to manage service requests (bookings):
- List & filter requests
- Search by patient name
- View request details
- Assign a nurse
- Cancel a request
- Send notifications
- Log admin activities

---

## Statuses Used
- Pending
- Assigned
- Completed
- Cancelled

> Note: Other modules may use (Accepted, Active, Rejected). Consider unifying later.

---

## Endpoints

### 1) Get Service Requests
**GET** `/api/admin/service-requests`

**Query Params**
- `search` (string, optional): patient name
- `status` (string, optional): Pending | Assigned | Completed | Cancelled | All

**Response**
```json
[
  {
    "requestId": 15,
    "patientName": "John Doe",
    "assignedNurse": "Sarah Hassan",
    "serviceType": "IV Therapy",
    "date": "2026-04-01",
    "time": "14:00",
    "status": "Assigned"
  }
]
```

---

### 2) Get Request Details
**GET** `/api/admin/service-requests/{bookingId}`

**Response**
```json
{
  "bookingId": 15,
  "patientName": "John Doe",
  "nurseName": "Sarah Hassan",
  "serviceType": "IV Therapy",
  "date": "2026-04-01",
  "time": "14:00",
  "status": "Assigned",
  "address": "Amman, Abdali",
  "notes": "Post-surgery care"
}
```

---

### 3) Assign Nurse
**PUT** `/api/admin/service-requests/{bookingId}/assign`

**Body**
```json
{
  "nurseId": "user-id"
}
```

**Behavior**
- Validate booking exists
- Validate nurse exists and role is Nurse
- Set:
  - `NurseId`
  - `Status = "Assigned"`
- Create Notification to nurse
- Create AdminActivityLog

**Response**
```json
{ "message": "Nurse assigned successfully" }
```

---

### 4) Cancel Request
**PUT** `/api/admin/service-requests/{bookingId}/cancel`

**Validation**
- Cannot cancel if status is `Completed` or `Cancelled`

**Behavior**
- Set `Status = "Cancelled"`
- Notify patient
- Notify assigned nurse (if any)
- Create AdminActivityLog

**Response**
```json
{ "message": "Request cancelled" }
```

---

## Notifications

### Assign
- Title: New Assigned Request
- To: Nurse

### Cancel
- To Patient:
  - Title: Service Request Cancelled
- To Nurse (if assigned):
  - Title: Assigned Request Cancelled

---

## Admin Activity Logs

### Assign
- Title: Nurse assigned to request

### Cancel
- Title: Service request cancelled

---

## Required Components

### DTO
`DTOs/AssignNurseDto.cs`
```csharp
public class AssignNurseDto
{
    public string NurseId { get; set; }
}
```

### Models Required
- Booking
- Notification
- AdminActivityLog

### DbContext
```csharp
public DbSet<AdminActivityLog> AdminActivityLogs { get; set; }
```

---

## Where to Place Code

### DTO
```
DTOs/AssignNurseDto.cs
```

### Controller Methods
```
Controllers/AdminController.cs
```

Group under:
```csharp
// Service Requests Management
```

---

## UI Mapping

| UI Action | Endpoint |
|----------|--------|
| Table load | GET /service-requests |
| Search | ?search= |
| Filter | ?status= |
| View | GET /{id} |
| Assign | PUT /assign |
| Cancel | PUT /cancel |

---

## Final Result
This module fully supports:
- Admin control over requests
- Real-time notifications
- Activity tracking
- Ready for frontend integration

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////