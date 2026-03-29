# Patient Profile / Onboarding Section Documentation

## Overview
This document explains the backend work added for the **Patient additional information screens** after account creation.

The goal is to support the following onboarding steps for the patient:

- Step 1: Account creation
- Step 2: Personal Info
- Step 3: Address (**optional**)
- Step 4: Medical Info

---

# 1) Why PatientProfile was added

Instead of storing all patient extra data directly inside `ApplicationUser`, a separate `PatientProfile` model was added.

## Benefits
- keeps user authentication data separate from profile data
- makes patient data more organized
- easier to extend later
- supports onboarding screens cleanly

---

# 2) PatientProfile Model

## File
`Models/PatientProfile.cs`

## Structure
```csharp
namespace NurseNow.Models
{
    public class PatientProfile
    {
        public int PatientProfileId { get; set; }

        public string UserId { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? BloodType { get; set; }

        public string? Governorate { get; set; }

        public string? Area { get; set; }

        public string? Address { get; set; }

        public string? Conditions { get; set; }

        public string? Allergies { get; set; }

        public string? Notes { get; set; }

        public ApplicationUser User { get; set; }
    }
}
```

---

# 3) ApplicationDbContext Setup

## File
`Data/ApplicationDbContext.cs`

## DbSet
```csharp
public DbSet<PatientProfile> PatientProfiles { get; set; }
```

## Relationship
```csharp
builder.Entity<PatientProfile>()
    .HasOne(p => p.User)
    .WithMany()
    .HasForeignKey(p => p.UserId)
    .OnDelete(DeleteBehavior.Cascade);
```

---

# 4) Register Flow Update

## File
`Controllers/AuthController.cs`

When a new user registers as `Patient`, the backend now creates an empty `PatientProfile`.

## Logic
```csharp
else if (model.Role == "Patient")
{
    var patientProfile = new PatientProfile
    {
        UserId = user.Id
    };

    _context.PatientProfiles.Add(patientProfile);
}

await _context.SaveChangesAsync();
```

## Result
Every patient account now has a related profile record ready for onboarding updates.

---

# 5) DTOs Added

## A) Personal Info DTO
### File
`DTOs/UpdatePatientPersonalInfoDto.cs`

```csharp
namespace NurseNow.DTOs
{
    public class UpdatePatientPersonalInfoDto
    {
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? BloodType { get; set; }
    }
}
```

---

## B) Address DTO
### File
`DTOs/UpdatePatientAddressDto.cs`

```csharp
namespace NurseNow.DTOs
{
    public class UpdatePatientAddressDto
    {
        public string? Governorate { get; set; }
        public string? Area { get; set; }
        public string? Address { get; set; }
    }
}
```

---

## C) Medical Info DTO
### File
`DTOs/UpdatePatientMedicalInfoDto.cs`

```csharp
namespace NurseNow.DTOs
{
    public class UpdatePatientMedicalInfoDto
    {
        public string? Conditions { get; set; }
        public string? Allergies { get; set; }
        public string? Notes { get; set; }
    }
}
```

---

# 6) PatientProfileController

## File
`Controllers/PatientProfileController.cs`

This controller was added to manage patient onboarding/profile data.

---

# 7) Endpoints

## A) Get full patient profile
```http
GET /api/patientprofile
```

## Purpose
Returns all patient profile data.

## Response Example
```json
{
  "fullName": "Ali Mohammed",
  "email": "ali@gmail.com",
  "gender": "Male",
  "dateOfBirth": "2001-05-10T00:00:00",
  "bloodType": "A+",
  "governorate": "Amman",
  "area": "Abdali",
  "address": "Street 10, Building 5",
  "conditions": "Diabetes",
  "allergies": "Penicillin",
  "notes": "Needs regular monitoring"
}
```

---

## B) Update Personal Info
```http
PUT /api/patientprofile/personal-info
```

## Used for
Step 2: Personal Info

## Request Example
```json
{
  "gender": "Male",
  "dateOfBirth": "2001-05-10",
  "bloodType": "A+"
}
```

## Purpose
Updates:
- gender
- date of birth
- blood type

---

## C) Update Address
```http
PUT /api/patientprofile/address
```

## Used for
Step 3: Address

## Request Example
```json
{
  "governorate": "Amman",
  "area": "Abdali",
  "address": "Street 10, Building 5"
}
```

## Purpose
Updates:
- governorate
- area
- address

---

## D) Update Medical Info
```http
PUT /api/patientprofile/medical-info
```

## Used for
Step 4: Medical Info

## Request Example
```json
{
  "conditions": "Diabetes",
  "allergies": "Penicillin",
  "notes": "Needs regular monitoring"
}
```

## Purpose
Updates:
- conditions
- allergies
- notes

---

# 8) Optional Step 3

Step 3 (Address) was intentionally designed as **optional**.

## Why?
Because all related fields in `PatientProfile` are nullable:

- `Governorate`
- `Area`
- `Address`

This means:
- the user can skip Step 3
- the backend will not fail
- no required validation is needed for address at this stage

---

# 9) Migration

After adding `PatientProfile` and updating the context:

```powershell
Add-Migration AddPatientProfile
Update-Database
```

---

# 10) Final Flow

## Step 1
Patient registers account

## Step 2
Patient updates personal info

## Step 3
Patient may optionally update address

## Step 4
Patient updates medical info

---

# 11) Summary of What Was Added

## Completed
- PatientProfile model
- PatientProfiles DbSet
- PatientProfile relationship
- automatic PatientProfile creation on patient registration
- DTOs for personal info, address, and medical info
- PatientProfileController
- full patient profile endpoint
- personal info update endpoint
- address update endpoint
- medical info update endpoint
- optional address step support

---

# 12) Notes

## Step 3 is optional
No backend validation forces the patient to complete address information.

## Better structure
Patient extra information is kept separate from authentication data.

## Future improvements
Later, governorate and area can be served from fixed dropdown APIs if needed.
*9*************************************************************************************************************************
# Rating and Reviews System Documentation

## Overview
This document explains the backend work implemented for the **Rating and Reviews System** in the project.

The goal of this feature is:
- allow the patient to rate the nurse after a completed appointment
- allow the patient to write an optional review comment
- allow the patient to dismiss the rating prompt permanently using `X`
- allow the patient to postpone the rating prompt using `Later`
- show average rating and reviews count in nurse profile and browse nurses
- show reviews list inside nurse profile

---

# 1) Review Flow

The review flow works only after the booking status becomes:

```text
Completed
```

After that, the patient may see a rating prompt.

The patient has 3 choices:

## A) Submit review
- selects rating
- optionally writes comment
- submits review
- prompt never appears again for that booking

## B) Dismiss using X
- prompt is hidden permanently for that booking
- no review is submitted

## C) Press Later
- prompt is hidden temporarily
- it can appear again later

---

# 2) Review Model

## File
`Models/Review.cs`

## Structure
```csharp
namespace NurseNow.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int BookingId { get; set; }

        public string PatientId { get; set; }

        public string NurseId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Booking Booking { get; set; }

        public ApplicationUser Patient { get; set; }

        public ApplicationUser Nurse { get; set; }
    }
}
```

---

# 3) Booking Model Updates

## File
`Models/Booking.cs`

The following fields were added:

```csharp
public bool IsReviewSubmitted { get; set; } = false;

public bool IsReviewDismissed { get; set; } = false;

public DateTime? ReviewRemindLaterAt { get; set; }
```

## Purpose
- `IsReviewSubmitted`: review already submitted
- `IsReviewDismissed`: prompt was closed permanently
- `ReviewRemindLaterAt`: reminder postponed until a future time

---

# 4) ApplicationDbContext Setup

## File
`Data/ApplicationDbContext.cs`

## DbSet
```csharp
public DbSet<Review> Reviews { get; set; }
```

## Relationships
```csharp
builder.Entity<Review>()
    .HasOne(r => r.Booking)
    .WithMany()
    .HasForeignKey(r => r.BookingId)
    .OnDelete(DeleteBehavior.Restrict);

builder.Entity<Review>()
    .HasOne(r => r.Patient)
    .WithMany()
    .HasForeignKey(r => r.PatientId)
    .OnDelete(DeleteBehavior.Restrict);

builder.Entity<Review>()
    .HasOne(r => r.Nurse)
    .WithMany()
    .HasForeignKey(r => r.NurseId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

# 5) Migration

After adding the review model and booking fields:

```powershell
Add-Migration AddReviewsAndBookingReviewFlags
Update-Database
```

---

# 6) Submit Review DTO

## File
`DTOs/SubmitReviewDto.cs`

```csharp
namespace NurseNow.DTOs
{
    public class SubmitReviewDto
    {
        public int BookingId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}
```

---

# 7) ReviewController

## File
`Controllers/ReviewController.cs`

This controller was added to manage review actions for patients.

---

# 8) Review Endpoints

## A) Submit Review
```http
POST /api/review
```

## Request Example
```json
{
  "bookingId": 15,
  "rating": 5,
  "comment": "Very professional nurse"
}
```

## Rules
- patient must own the booking
- booking must be `Completed`
- rating must be between 1 and 5
- booking must not already have submitted review
- booking must not be dismissed

## Result
- review is saved
- `IsReviewSubmitted = true`
- `ReviewRemindLaterAt = null`

---

## B) Dismiss Review Prompt
```http
PUT /api/review/{bookingId}/dismiss
```

## Purpose
Used when patient presses `X`.

## Result
- `IsReviewDismissed = true`
- `ReviewRemindLaterAt = null`
- prompt will never appear again for this booking

---

## C) Remind Later
```http
PUT /api/review/{bookingId}/later
```

## Purpose
Used when patient presses `Later`.

## Result
- `ReviewRemindLaterAt = DateTime.UtcNow.AddDays(1)`
- prompt is postponed temporarily

---

## D) Get Pending Review Prompt
```http
GET /api/review/pending
```

## Purpose
Returns one completed booking that still needs a review prompt.

## Conditions
The booking is returned only if:
- status = `Completed`
- `IsReviewSubmitted = false`
- `IsReviewDismissed = false`
- `ReviewRemindLaterAt` is null or expired

## Example Response
```json
{
  "bookingId": 15,
  "nurseId": "nurse-user-id",
  "nurseName": "Sarah Hassan",
  "serviceName": "IV Therapy",
  "date": "2026-03-29",
  "time": "10:00"
}
```

## If no booking needs review
The endpoint returns:
```json
null
```

---

# 9) Nurse Reviews Endpoint

## File
`PatientController.cs`

## Endpoint
```http
GET /api/patient/nurses/{nurseId}/reviews
```

## Purpose
Returns the nurse reviews page data.

## Returned Data
- `averageRating`
- `reviewsCount`
- `reviews`

## Example Response
```json
{
  "averageRating": 4.7,
  "reviewsCount": 3,
  "reviews": [
    {
      "reviewId": 1,
      "patientName": "Ali Mohammed",
      "rating": 5,
      "comment": "Very professional nurse",
      "createdAt": "2026-03-29T10:30:00Z"
    },
    {
      "reviewId": 2,
      "patientName": "Ahmad Saleh",
      "rating": 4,
      "comment": "Good service",
      "createdAt": "2026-03-28T09:00:00Z"
    }
  ]
}
```

---

# 10) Nurse Details Endpoint Update

## File
`PatientController.cs`

## Endpoint
```http
GET /api/patient/nurses/{nurseId}
```

This endpoint was updated to return real:
- `rating`
- `reviewsCount`

instead of placeholder values.

## Current Logic
- `rating` = average of all review ratings for the nurse
- `reviewsCount` = total number of reviews for the nurse

---

# 11) Browse Nurses Endpoint Update

## File
`PatientController.cs`

## Endpoint
```http
GET /api/patient/nurses/browse
```

This endpoint was updated to return real:
- `rating`
- `reviewsCount`

for each nurse in the list.

## Current Logic
- average rating is calculated from `Reviews`
- total reviews count is calculated from `Reviews`

---

# 12) Rules Summary

## Patient can submit review only if:
- booking belongs to that patient
- booking status is `Completed`
- rating is between 1 and 5
- review not submitted before
- review not dismissed before

## If patient presses X
- prompt disappears permanently

## If patient presses Later
- prompt is hidden temporarily

---

# 13) Flutter Usage

## Show review popup
Call:
```http
GET /api/review/pending
```

### If response is null
- do not show popup

### If response contains booking
- show rating popup

---

## Submit review
Call:
```http
POST /api/review
```

---

## Dismiss permanently
Call:
```http
PUT /api/review/{bookingId}/dismiss
```

---

## Remind later
Call:
```http
PUT /api/review/{bookingId}/later
```

---

## Load nurse reviews page
Call:
```http
GET /api/patient/nurses/{nurseId}/reviews
```

---

# 14) What Was Implemented

## Completed
- Review model
- review fields inside booking
- review relationships in DbContext
- review migration
- submit review DTO
- ReviewController
- submit review endpoint
- dismiss review endpoint
- remind later endpoint
- pending review endpoint
- nurse reviews endpoint
- real average rating in nurse details
- real average rating in browse nurses
- real reviews count in nurse details
- real reviews count in browse nurses

---

# 15) Final Note

The rating system is now fully integrated with:
- completed bookings
- patient review actions
- nurse profile ratings
- nurse reviews list
- browse nurses rating display

This makes the rating and review flow part of the actual booking lifecycle.
*************************************************************************************
# Patient Dashboard API Documentation

## Overview
This section explains the APIs used to build the **Patient Dashboard**.

The dashboard includes:
- Total Bookings
- Active Requests
- Upcoming Appointments (limited number)

---

# 1) Dashboard Summary Endpoint

## Endpoint
```http
GET /api/patient/dashboard-summary
```

## Purpose
Returns quick statistics for the dashboard.

## Response
```json
{
  "totalBookings": 12,
  "activeRequests": 3
}
```

## Fields Explanation

### totalBookings
Total number of bookings created by the patient.

### activeRequests
Bookings that are still active:
- Pending
- Accepted
- Active

---

# 2) Appointments Endpoint (Updated)

## Endpoint
```http
GET /api/patient/appointments
```

## Query Parameters

### tab
حدد نوع البيانات:
- `upcoming`
- `past`

### limit (optional)
عدد النتائج التي تريد إرجاعها (مفيد للداش بورد)

---

# 3) Usage Examples

## Get all upcoming appointments
```http
GET /api/patient/appointments?tab=upcoming
```

## Get only 2 upcoming appointments (Dashboard)
```http
GET /api/patient/appointments?tab=upcoming&limit=2
```

## Get only 3 upcoming appointments
```http
GET /api/patient/appointments?tab=upcoming&limit=3
```

---

# 4) Response Example

```json
[
  {
    "bookingId": 15,
    "nurseName": "Sarah Hassan",
    "profileImageUrl": "https://api.com/uploads/image.jpg",
    "serviceName": "IV Therapy",
    "date": "2026-03-29",
    "time": "14:00",
    "address": "Amman",
    "totalPrice": 25,
    "status": "Accepted"
  }
]
```

---

# 5) Dashboard Flow

## Step 1: Load Summary
```http
GET /api/patient/dashboard-summary
```

## Step 2: Load Upcoming Appointments (Limited)
```http
GET /api/patient/appointments?tab=upcoming&limit=2
```

---

# 6) Notes

- `limit` is optional and used only for UI optimization.
- Sorting is done by:
  - BookingDate
  - StartTime
- Same endpoint is reused for:
  - Dashboard
  - Full appointments page

---

# 7) What Was Implemented

- Dashboard summary endpoint
- Active requests calculation
- Total bookings calculation
- Limit support in appointments endpoint
- Reuse of existing endpoint instead of duplication

---

# 8) Final Result

The dashboard is now:
- Efficient
- Clean (no duplicated endpoints)
- Flexible for frontend usage
