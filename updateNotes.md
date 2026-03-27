# Notifications Section Documentation

## Overview
This document explains the backend notification system implemented in the project.

The notification system currently supports:
- nurse account approval / rejection notifications
- new service request notification for nurse
- booking accepted notification for patient
- booking rejected notification for patient
- payment successful notification for patient
- payment received notification for nurse
- appointment cancelled notification
- appointment completed notification

It also supports:
- getting all notifications
- marking one notification as read
- marking all notifications as read
- returning target screen information for frontend navigation

---

# 1) Notification Model

## File
`Models/Notification.cs`

## Structure
```csharp
namespace NurseNow.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public string UserId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Type { get; set; }

        public int? BookingId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
    }
}
```

## Purpose of Fields
- `UserId`: who receives the notification
- `Title`: short notification title
- `Message`: detailed notification message
- `Type`: category such as `Request`, `Booking`, or `Payment`
- `BookingId`: related booking if applicable
- `IsRead`: read/unread state
- `CreatedAt`: creation timestamp

---

# 2) ApplicationDbContext Setup

## File
`Data/ApplicationDbContext.cs`

## DbSet
```csharp
public DbSet<Notification> Notifications { get; set; }
```

## Relationship
```csharp
builder.Entity<Notification>()
    .HasOne(n => n.User)
    .WithMany()
    .HasForeignKey(n => n.UserId)
    .OnDelete(DeleteBehavior.Cascade);
```

---

# 3) Migration

After adding the notification model:

```powershell
Add-Migration AddNotifications
Update-Database
```

---

# 4) NotificationController

## File
`Controllers/NotificationController.cs`

## Endpoints

### A) Get all notifications
```http
GET /api/notification
```

### B) Mark one notification as read
```http
PUT /api/notification/{id}/read
```

### C) Mark all notifications as read
```http
PUT /api/notification/read-all
```

---

# 5) Get Notifications Response

The notifications list returns:
- `NotificationId`
- `Title`
- `Message`
- `Type`
- `BookingId`
- `IsRead`
- `CreatedAt`
- `targetScreen`

## Example Response
```json
[
  {
    "notificationId": 1,
    "title": "New Service Request",
    "message": "You have received a new service request from a patient.",
    "type": "Request",
    "bookingId": 15,
    "isRead": false,
    "createdAt": "2026-03-27T10:30:00Z",
    "targetScreen": "RequestDetails"
  },
  {
    "notificationId": 2,
    "title": "Payment Received",
    "message": "The patient has completed the payment for the appointment.",
    "type": "Payment",
    "bookingId": 15,
    "isRead": false,
    "createdAt": "2026-03-27T11:00:00Z",
    "targetScreen": "AppointmentDetails"
  }
]
```

---

# 6) targetScreen Mapping

The backend returns `targetScreen` so Flutter knows where to navigate.

## Current Mapping
- `Request` → `RequestDetails`
- `Payment` → `AppointmentDetails`
- `Booking` → `AppointmentDetails`
- `Account` → `null`

---

# 7) Where Notifications Are Created

## A) AdminController
### Nurse account approval / rejection
When admin verifies a nurse:
- `Approved` → notification title: `Account Approved`
- `Rejected` → notification title: `Account Rejected`

Receiver:
- nurse

Type:
- `Account`

---

## B) PatientController
### CreateBookingRequest
When patient submits a new booking request:
- notification title: `New Service Request`

Receiver:
- nurse

Type:
- `Request`

### ConfirmPayment
When patient completes payment:
- notification title: `Payment Successful`

Receiver:
- patient

Type:
- `Payment`

And also:
- notification title: `Payment Received`

Receiver:
- nurse

Type:
- `Payment`

### CancelAppointment
When patient cancels appointment:
- notification title: `Appointment Cancelled`

Receiver:
- nurse

Type:
- `Booking`

---

## C) NurseController
### AcceptRequest
When nurse accepts request:
- notification title: `Booking Confirmed`

Receiver:
- patient

Type:
- `Booking`

### DeclineRequest
When nurse rejects request:
- notification title: `Request Rejected`

Receiver:
- patient

Type:
- `Booking`

### CompleteAppointment
When nurse completes appointment:
- notification title: `Appointment Completed`

Receiver:
- patient

Type:
- `Booking`

### CancelAppointmentByNurse
When nurse cancels appointment:
- notification title: `Appointment Cancelled`

Receiver:
- patient

Type:
- `Booking`

---

# 8) Read Logic

## Mark one as read
The endpoint:
```http
PUT /api/notification/{id}/read
```
changes:
```text
IsRead = true
```
for a single notification.

## Mark all as read
The endpoint:
```http
PUT /api/notification/read-all
```
changes:
```text
IsRead = true
```
for all unread notifications of the logged-in user.

---

# 9) Flutter Usage

## Open Notifications Screen
Call:
```http
GET /api/notification
```

## Tap one notification
Use:
- `bookingId`
- `targetScreen`

### If targetScreen = RequestDetails
Navigate to:
- request details screen

### If targetScreen = AppointmentDetails
Navigate to:
- appointment details screen

## Mark one as read
Call:
```http
PUT /api/notification/{id}/read
```

## Mark all as read
Call:
```http
PUT /api/notification/read-all
```

---

# 10) What Was Implemented

## Completed
- Notification model
- Notification DbSet
- Notification relationship
- Get notifications endpoint
- Mark one as read endpoint
- Mark all as read endpoint
- targetScreen mapping
- account verification notifications
- new service request notification
- booking accepted notification
- booking rejected notification
- payment successful notification
- payment received notification
- appointment cancelled notification
- appointment completed notification

---

# 11) Deferred for Later

The only notification item intentionally postponed is:

## Appointment Reminder
This was postponed because automatic reminder notifications need:
- background job
- scheduler
- or push notification service

Examples:
- Hangfire
- Quartz.NET
- BackgroundService
- Firebase push notifications

This is different from normal notifications because it is time-based, not action-based.

---

# 12) Final Note

The current notification system is fully usable for the core app flow.

It already supports:
- patient notifications
- nurse notifications
- admin-triggered notifications
- navigation from notification card to the correct screen


---

# 13) Appointment Reminder (مؤجل حاليًا)

## ما هو Appointment Reminder؟
هو إشعار يتم إرساله للمستخدم (المريض أو الممرض) قبل موعد الحجز بفترة معينة، مثل:
- قبل الموعد بيوم
- قبل الموعد بساعة

مثال:
"You have an appointment tomorrow at 10:00 AM"

---

## لماذا لم نقم بتنفيذه الآن؟

تم تأجيل هذا الجزء لأنه يختلف عن باقي الإشعارات.

جميع الإشعارات التي قمنا بتنفيذها تعتمد على **Action مباشر** مثل:
- إنشاء حجز
- قبول الطلب
- الدفع
- إلغاء الموعد

أما الـ Appointment Reminder فهو يعتمد على **الوقت** وليس على حدث مباشر.

---

## ما المشكلة التقنية؟

لكي يعمل Reminder بشكل صحيح، يجب أن يقوم السيرفر بـ:
- مراقبة المواعيد بشكل مستمر
- التحقق من الوقت الحالي
- تحديد هل يوجد موعد قريب يحتاج Reminder
- إنشاء Notification تلقائيًا

وهذا يتطلب تشغيل كود في الخلفية بشكل دوري (بدون طلب من المستخدم).

---

## ما الذي نحتاجه لتنفيذه؟

نحتاج إلى ما يسمى:

### Background Job أو Scheduler

وهو نظام يقوم بتشغيل كود بشكل تلقائي كل فترة (مثلاً كل دقيقة أو كل 5 دقائق).

---

## أمثلة على حلول في .NET

- Hangfire
- Quartz.NET
- BackgroundService

أو باستخدام:
- Firebase Push Notifications (للموبايل)

---

## لماذا لم نستخدمه الآن؟

لأن:
- يزيد من تعقيد المشروع
- يحتاج إعدادات إضافية
- يحتاج إدارة دقيقة للوقت
- يحتاج منع تكرار الإشعارات

وحاليًا نحن نركز على إنهاء الـ Core System أولاً.

---

## كيف يمكن تنفيذه لاحقًا؟

الخطوات العامة:

1. إنشاء Background Job يعمل كل فترة (مثلاً كل دقيقة)
2. جلب المواعيد القادمة من قاعدة البيانات
3. مقارنة الوقت الحالي مع وقت الموعد
4. إذا كان الموعد قريب (مثلاً خلال ساعة):
   - يتم إنشاء Notification
5. التأكد من عدم إرسال نفس Reminder أكثر من مرة

---

## ملاحظة

يمكن حاليًا عمل Reminder بشكل مؤقت من جهة الـ Frontend (Flutter)، مثل:
- عرض تنبيه داخل التطبيق بناءً على الوقت

لكن هذا ليس Reminder حقيقي من السيرفر.

---

## الخلاصة

تم تأجيل Appointment Reminder لأنه:
- يعتمد على الوقت وليس على حدث
- يحتاج Background Processing
- ليس ضروري لإكمال الوظائف الأساسية للنظام

وسيتم إضافته لاحقًا كـ تحسين (Enhancement) للنظام.