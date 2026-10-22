# ReadingSpot – שרת (ASP.NET Core Web API)

מערכת להזמנת מקומות לימוד בספרייה לפי חלונות זמן. לכל אזור לימוד (חדר שקט, אזור קבוצתי, עמדות מחשב…) יש מספר מקומות מוגבל, ולכל אזור יש חלונות זמן של שעתיים. משתמשת רשומה בוחרת אזור ותאריך ומזמינה מקום בחלון. מנהלת מוסיפה ומשביתה אזורים וחלונות, ורואה את כל ההזמנות.

לקוח React נמצא ב-repository נפרד: `readingspot-client`.

## המשאב המוגבל וטיפול בתחרות

המשאב שמתחרים עליו הוא **מקומות בחלון זמן**: השדה `TimeSlot.BookedCount`, שעולה באחד בכל הזמנה.

> שתי משתמשות לוחצות "הזמיני" על המקום האחרון באותו רגע. אסור ששתיהן יקבלו אישור.

הפתרון הוא **Optimistic Concurrency**:

- ל-`TimeSlot` יש שדה `Version` (Guid) עם `[ConcurrencyCheck]`.
- `AppDbContext.SaveChangesAsync` מחליף את ה-Version בכל עדכון. זה המקום היחיד שעושה זאת.
- EF מוסיף לפקודת ה-UPDATE את התנאי `WHERE Version = @old`. אם משתמשת אחרת שמרה קודם, אף שורה לא מתעדכנת, ונזרקת `DbUpdateConcurrencyException`.
- `Repository<T>.SaveChangesAsync` מתרגם אותה ל-`ConflictException`. ה-`ExceptionMiddleware` מחזיר **409** עם ההודעה "המקום נתפס על ידי משתמש אחר".
- הבדיקה העסקית (`BookedCount < Capacity`), העלאת המונה ויצירת ההזמנה נשמרות ב-`SaveChanges` אחד. EF עוטף אותו בטרנזקציה.

בחרנו `Guid` ולא `[Timestamp]` כי הוא עובד על כל בסיס נתונים, כולל SQLite. כך בדיקת התחרות (`ConcurrencyTests`) מריצה בדיוק את המנגנון שרץ בייצור.

## מבנה ה-Solution

| פרויקט | תפקיד | מפנה אל |
|---|---|---|
| `ReadingSpot.Core` | ישויות, Enums, DTOs, ממשקים, חריגות. **אפס חבילות חיצוניות** | — |
| `ReadingSpot.Data` | `AppDbContext`, Fluent API, Repositories, Migrations, Seed | Core |
| `ReadingSpot.Service` | לוגיקה עסקית, AutoMapper, JWT | Core |
| `ReadingSpot.API` | Controllers, Middleware, הגדרות, NLog | Core, Data, Service |
| `ReadingSpot.Tests` | xUnit + Moq + SQLite in-memory | Core, Data, Service |

## הרצה

**דרישות:** ‏.NET 8 SDK ו-SQL Server LocalDB (מותקן עם Visual Studio).

```bash
# 1. הסודות נשמרים מחוץ לקוד (User Secrets)
dotnet user-secrets set "ConnectionStrings:Default" "Server=(localdb)\MSSQLLocalDB;Database=ReadingSpot;Trusted_Connection=True;TrustServerCertificate=True" --project ReadingSpot.API
dotnet user-secrets set "Jwt:Key" "<מפתח אקראי באורך 32 תווים לפחות>" --project ReadingSpot.API

# 2. הרצה. ה-Migrations וה-Seed רצים אוטומטית בעליית השרת
dotnet run --project ReadingSpot.API --launch-profile https
```

- Swagger: <https://localhost:7180/swagger> (או <http://localhost:5180/swagger>)
- בעליית השרת נוצרים אוטומטית חלונות זמן ל-7 הימים הקרובים. הפעולה idempotent, ואפשר להריץ אותה שוב בלי כפילויות.

**כלי EF** (רק ליצירת Migrations חדשים): `dotnet tool restore`, ואז הפקודות שבסעיף "העלאה לענן".

## העלאה לענן (Render)

בענן השרת רץ ב-**Docker** (קובץ `Dockerfile` בשורש), ובסיס הנתונים הוא **PostgreSQL** של Render. מקומית ממשיכים לעבוד עם SQL Server. הבחירה נעשית בהגדרה אחת, `Database:Provider`.

**התקנה:** הקובץ `render.yaml` מגדיר את כל שלושת החלקים: בסיס הנתונים, השרת והלקוח. ב-Render בוחרים **New → Blueprint**, בוחרים את ה-repository הזה ולוחצים **Apply**. מפתח ה-JWT נוצר אוטומטית.

- שרת: <https://readingspot-yehudit-api.onrender.com/swagger>
- לקוח: <https://readingspot-yehudit.onrender.com>

ההגדרות שהקובץ מכניס:

| הגדרה | מקומית | Render (משתני סביבה) |
|---|---|---|
| ספק בסיס הנתונים | `SqlServer` (ברירת מחדל) | `Database__Provider=Postgres` |
| Connection string | User Secrets | `ConnectionStrings__Default=<Internal Database URL>` |
| מפתח JWT | User Secrets | `Jwt__Key` (נוצר אוטומטית) |
| כתובת הלקוח (CORS) | `localhost` | `Cors__Origins__0=https://<הלקוח>.onrender.com` |

- לכל ספק יש Migrations נפרדים: SQL Server ב-`Migrations/`, ו-PostgreSQL ב-`Migrations/Postgres/` (דרך `PostgresDbContext`, שיורש מ-`AppDbContext`, כך שהמודל והקונפיגורציה משותפים).
- ה-Migrations וה-Seed רצים אוטומטית בעליית השרת גם בענן. ב-PostgreSQL, `DbInitializer` גם מיישר את מוני ה-Identity אחרי ה-Seed.
- Render נותן את כתובת בסיס הנתונים בפורמט `postgresql://...`. `PostgresDbContext.NormalizeConnectionString` ממיר אותה לפורמט של Npgsql.

כשיש שני DbContext, פקודות `dotnet ef` צריכות לציין `--context`:

```bash
dotnet ef migrations add <Name> --context AppDbContext      --project ReadingSpot.Data --startup-project ReadingSpot.Data --output-dir Migrations
dotnet ef migrations add <Name> --context PostgresDbContext --project ReadingSpot.Data --startup-project ReadingSpot.Data --output-dir Migrations/Postgres
```

### משתמשי דמו

| מייל | סיסמה | תפקיד |
|---|---|---|
| `admin@readingspot.local` | `Admin123!` | Admin |
| `member@readingspot.local` | `Member123!` | Member |

## בדיקות

```bash
dotnet test
```

- **`BookingServiceTests`** (Moq): יצירה תקינה, חלון מלא, התנגשות מה-Repository, הזמנה כפולה, אזור לא פעיל, חלון שעבר, ביטול של משתמשת אחרת (403), ביטול תקין.
- **`AuthServiceTests`**: סיסמה שגויה, התחברות תקינה, מייל קיים.
- **`SpotServiceTests`**: מחיקה היא השבתה.
- **`ConcurrencyTests`** (SQLite in-memory, שני `DbContext` על אותו חיבור): שתי "בקשות" קוראות את המקום האחרון. הראשונה נשמרת, השנייה נכשלת, ו-`BookedCount` נשאר 2 ולא 3.

## Endpoints

| Method | Route | הרשאה |
|---|---|---|
| POST | `/api/auth/register`, `/api/auth/login` | כולם |
| GET | `/api/spots?page&pageSize&search&amenityId&sortBy` | מחוברת |
| GET | `/api/spots/{id}` | מחוברת |
| POST / PUT / DELETE | `/api/spots`, `/api/spots/{id}` (DELETE = השבתה) | Admin |
| GET | `/api/spots/{spotId}/timeslots?date=yyyy-MM-dd` | מחוברת |
| POST | `/api/spots/{spotId}/timeslots` | Admin |
| GET | `/api/amenities` | מחוברת |
| GET | `/api/bookings?page&pageSize` | מחוברת (Admin רואה הכול) |
| GET / DELETE | `/api/bookings/{id}` | בעלת ההזמנה / Admin |
| POST | `/api/bookings` | מחוברת |

כל שגיאה חוזרת באותו פורמט: `{ status, message, correlationId }`.

## לוגים (NLog)

- הלוגים נכתבים לקונסול ולקובץ `logs/<תאריך>.log`.
- בכל שורה מופיע ה-CorrelationId של הבקשה. הלקוח יכול לשלוח אותו ב-header ‏`X-Correlation-Id`, והוא חוזר באותו header בתשובה.
- **Information**: כל בקשה, הזמנה שנוצרה או בוטלה.
- **Warning**: התחברות כושלת (רק המייל), התנגשות על חלון זמן.
- **Error**: חריגה לא צפויה, ב-`ExceptionMiddleware` בלבד.
- סיסמאות ו-tokens לעולם לא נרשמים.

## הערה על DI

`AppDbContext` וכל ה-Repositories וה-Services רשומים כ-**Scoped**, כלומר מופע אחד לכל בקשת HTTP. אם שירות **Singleton** היה מקבל שירות Scoped בבנאי, הוא היה "תופס" מופע אחד של ה-DbContext לכל חיי האפליקציה, ובקשות שונות היו חולקות אותו. זו בעיית **captive dependency**: נתונים "דולפים" בין בקשות, ומתקבלות שגיאות מקביליות, כי DbContext אינו thread-safe.

## עיקרון: כל לוגיקה במקום אחד

| לוגיקה | המקום היחיד |
|---|---|
| החלפת Version | `AppDbContext.SaveChangesAsync` |
| תרגום `DbUpdateConcurrencyException` | `Repository<T>.SaveChangesAsync` |
| מיפוי חריגות לסטטוס HTTP | `ExceptionMiddleware` (+ `ErrorResponse.WriteAsync`) |
| Pagination | `ToPagedAsync` |
| מיפוי DTOs | `MappingProfile` |
| פענוח זהות המשתמשת | `ClaimsPrincipalExtensions` |
| רישום DI | `ServiceCollectionExtensions` |
| נתוני בדיקה | `TestData` |
