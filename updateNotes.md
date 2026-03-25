# Payment Flow Documentation

## Overview
This document explains the backend payment flow implemented for the project using **Stripe Payment Intents**.

The chosen payment approach is:
- custom payment UI in Flutter
- card/payment method selection in the app
- backend creates Stripe Payment Intent
- frontend confirms the payment
- backend finalizes payment status after success

---

# 1) Payment Flow Summary

The payment flow works like this:

1. Patient opens the payment page by clicking **Pay Now**
2. Backend returns payment summary
3. Patient selects payment method and confirms payment
4. Backend creates Stripe Payment Intent
5. Flutter confirms payment using Stripe SDK
6. If payment succeeds:
   - backend confirms payment
   - payment status becomes `Paid`
   - booking status becomes `Active`
   - Flutter opens payment success screen
7. If payment fails:
   - Flutter opens payment failed screen
   - patient can retry payment

---

# 2) Stripe Setup

## appsettings.json
The project stores Stripe secret key in:

```json
"Stripe": {
  "SecretKey": "sk_test_xxxxxxxxxxxxxxxxxxxxx"
}
```

---

## Program.cs
Stripe API key is initialized using:

```csharp
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
```

---

## NuGet Package
Required package:

```bash
dotnet add package Stripe.net
```

---

# 3) Payment Model

## Payment.cs
```csharp
namespace NurseNow.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int BookingId { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Booking Booking { get; set; }
    }
}
```

### Payment Statuses
- `Pending`
- `Paid`
- `Failed` (optional for future use)

---

# 4) Booking Relationship

## Booking.cs
The booking model includes:

```csharp
public Payment? Payment { get; set; }
```

---

## ApplicationDbContext
The payment table is registered using:

```csharp
public DbSet<Payment> Payments { get; set; }
```

and the one-to-one relationship is configured as:

```csharp
builder.Entity<Payment>()
    .HasOne(p => p.Booking)
    .WithOne(b => b.Payment)
    .HasForeignKey<Payment>(p => p.BookingId)
    .OnDelete(DeleteBehavior.Cascade);
```

---

# 5) Migration

After adding the payment model and relationship:

```powershell
Add-Migration AddPaymentTable
Update-Database
```

---

# 6) Payment Summary API

## Endpoint
`GET /api/patient/payments/summary/{bookingId}`

## Purpose
Returns the payment page summary before the patient confirms payment.

## Response Example
```json
{
  "nurseName": "Sarah Hassan",
  "serviceName": "IV Therapy",
  "date": "2026-03-11",
  "time": "10:00",
  "amount": 50.0
}
```

---

# 7) Create Payment Intent API

## Endpoint
`POST /api/patient/payments/create-intent/{bookingId}`

## Purpose
Creates a Stripe Payment Intent and returns `clientSecret` to Flutter.

## Validation Rules
- booking must belong to logged-in patient
- booking must exist
- booking status must be `Accepted`
- booking must not already have a paid payment

## Response Example
```json
{
  "clientSecret": "pi_xxx_secret_xxx"
}
```

---

# 8) Confirm Payment API

## Endpoint
`POST /api/patient/payments/confirm/{bookingId}`

## Purpose
Confirms payment internally in the project after Flutter reports successful Stripe payment.

## Backend Logic
The endpoint:
- checks booking ownership
- checks existing payment
- creates or updates `Payment`
- sets `Payment.Status = Paid`
- updates `Booking.Status = Active`

## Response Example
```json
{
  "message": "Payment confirmed successfully.",
  "bookingId": 15,
  "amountPaid": 50.0,
  "paymentStatus": "Paid",
  "bookingStatus": "Active"
}
```

---

# 9) Get Payment API

## Endpoint
`GET /api/patient/payments/{bookingId}`

## Purpose
Returns stored payment information for a booking.

## Response Example
```json
{
  "paymentId": 3,
  "bookingId": 15,
  "amount": 50.0,
  "status": "Paid",
  "createdAt": "2026-03-26T10:30:00Z"
}
```

---

# 10) Success and Failed Screens

## Payment Success Screen
This screen does not need a separate backend endpoint.

It uses the response from:

`POST /api/patient/payments/confirm/{bookingId}`

Displayed data:
- amount paid
- booking id

## Payment Failed Screen
This screen also does not require a separate backend endpoint for the current version.

If Stripe payment fails:
- Flutter opens the failed screen directly
- the user can retry payment

### Retry Payment
Retry simply repeats:
- create payment intent
- confirm payment in Flutter

---

# 11) Flutter Integration Flow

## Step 1
Call:

```http
GET /api/patient/payments/summary/{bookingId}
```

## Step 2
Call:

```http
POST /api/patient/payments/create-intent/{bookingId}
```

## Step 3
Use Stripe SDK in Flutter to confirm payment.

## Step 4
If success:
call

```http
POST /api/patient/payments/confirm/{bookingId}
```

and open success screen.

## Step 5
If failed:
open failed screen and allow retry.

---

# 12) Important Notes

## Payment Availability
Payment is allowed only when:
- booking status = `Accepted`

## After successful payment
The booking status is changed to:
- `Active`

## Test Mode
This payment flow uses Stripe test mode, not live mode.

---

# 13) Status

✔ Stripe setup documented  
✔ Payment model added  
✔ Payment summary API added  
✔ Create payment intent API added  
✔ Confirm payment API added  
✔ Get payment API added  
✔ Success / failed flow defined  

⏳ Pending:
- real webhook verification
- optional failed payment persistence
- Apple Pay integration details
