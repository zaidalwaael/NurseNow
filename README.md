# NurseNow — four-layer refactor

ابدأ من `NurseNow.sln`، واجعل `NurseNow.API` هو Startup Project.

تم فصل الـ104 actions الفعّالة إلى Controllers رفيعة و23 Application Services، مع بقاء مساراتها وأدوار الوصول الأصلية. أضيفت حماية الملفات وhealth endpoint. النسخة مبنية على .NET 8 وSQL Server.

هذه Clean Architecture عملية وليست نسخة خالية تمامًا من أطر العمل: Application تحتفظ بـEF LINQ عبر interface وUserManager؛ وApplicationUser يحتفظ بوراثة IdentityUser في Domain. التفاصيل والحدود في `docs/ARCHITECTURE.md`.

## التشغيل — PowerShell من مجلد الحل

```powershell
dotnet restore NurseNow.sln
dotnet build NurseNow.sln
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=NurseNow;Trusted_Connection=True;TrustServerCertificate=True" --project src/NurseNow.API
```

ولّد مفتاح JWT محليًا ثم احفظه. لا تنسخه داخل ملفات المشروع:

```powershell
$jwtKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set "JWT:Secret" $jwtKey --project src/NurseNow.API
```

عدّل `PublicBaseUrl` و`Cors:AllowedOrigins` و`AllowedHosts` لتطابق بيئتك. للدفع أضف مفتاح Stripe التجريبي عبر user-secrets إلى `Stripe:SecretKey`. إعدادات البريد أيضًا خارج الكود.

## قاعدة البيانات

خُذ نسخة احتياطية، وجرّب على قاعدة اختبار أولًا. لم تُشغّل أي migration على بياناتك.

للنسخة الحالية من قاعدة المشروع، راجع `database/upgrade-from-original.sql`. يفترض هذا السكربت أن آخر migration مطبّق هو `20260425181715_RemoveBookingIdFromComplaints`. افحص `__EFMigrationsHistory` أولًا؛ إذا كانت القاعدة أقدم، أنشئ سكربتًا يبدأ من آخر migration مطبّق عندك.

الـmigration الجديدة تضيف معرف Stripe وفهارس، وتمنع تكرار review للحجز. تتوقف إذا وجدت reviews مكررة أو DayOfWeek أطول من 9 أحرف؛ لا تحذف البيانات ولا تختصرها تلقائيًا.

يمكن توليد سكربت على جهازك:

```powershell
dotnet ef migrations script --idempotent --project src/NurseNow.Infrastructure --startup-project src/NurseNow.API --output database/review-before-running.sql
```

لمراجعة أو تطبيق migrations باستخدام CLI، الـDesignTimeDbContextFactory يقرأ اتصال قاعدة البيانات من environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=YOUR_SERVER;Database=NurseNow;Trusted_Connection=True;TrustServerCertificate=True"
# نفّذ الأمر التالي على قاعدة الاختبار بعد مراجعة السكربت فقط:
dotnet ef database update --project src/NurseNow.Infrastructure --startup-project src/NurseNow.API
```

الـmigrations القديمة محفوظة؛ لم تُحذف أو يُعاد ترقيمها. لمسار قاعدة فارغة، راجع سكربت سلسلة migrations كاملة واختبره قبل استخدامها. لم تُختبر السلسلة التاريخية على SQL Server هنا.

## الأدوار وأول مدير

التسجيل يحتاج وجود Patient وNurse وAdministrator. لتجهيزها، فعّل seeding مرة واحدة. لإنشاء مدير جديد أضف بياناته صراحة:

```powershell
dotnet user-secrets set "Database:SeedOnStartup" "true" --project src/NurseNow.API
dotnet user-secrets set "SeedAdmin:Email" "YOUR_ADMIN_EMAIL" --project src/NurseNow.API
dotnet user-secrets set "SeedAdmin:Password" "YOUR_STRONG_PASSWORD" --project src/NurseNow.API
dotnet run --project src/NurseNow.API
```

بعد نجاح التشغيل، اجعل `Database:SeedOnStartup` تساوي false واحذف `SeedAdmin:Password`. لا توجد كلمة مرور مدير جاهزة. المدير الموجود مسبقًا لا يُرفع تلقائيًا إلى SuperAdmin؛ راجع قيمة AdminRole للحساب الصحيح.

Swagger: `https://localhost:7140/swagger` في Development فقط.

## الاختبارات

من مجلد الحل:

```powershell
dotnet run --project tests/NurseNow.RegressionTests -- docs/endpoint-baseline.json
```

هذا مشروع executable للاختبارات وليس xUnit؛ فشل أي فحص ينهي العملية بخطأ. يستخدم SQLite وقناة دفع مزيفة. اقرأ `docs/VALIDATION.md` للنتائج وحدودها.

## قبل ربط الفرونت

اقرأ `docs/COMPATIBILITY.md`؛ أهم التغييرات: تسجيل دخول جديد مطلوب للتوكنات القديمة، ملفات الصور والوثائق تحتاج Authorization header، تأكيد الدفع يحتاج Payment Intent تم إنشاؤه وتأكيده لدى Stripe، وكلمات المرور الجديدة طولها 12 على الأقل.

اقرأ `docs/CHANGES.md` للتعديلات، و`docs/KNOWN_LIMITATIONS.md` لما يحتاج تحققًا أو تطويرًا لاحقًا.
