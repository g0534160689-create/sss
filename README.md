# דעת אהרן — בנייה מחדש

פרויקט Windows חדש שנבנה מאפס. נשמרת מערכת הקישורים והרעיון הפונקציונלי; לא מועתק קוד מהפרויקט הקודם.

## טכנולוגיות
- C# / .NET 10
- WPF + XAML
- WebView2
- SQLite
- MVVM
- xUnit ו-GitHub Actions

## מבנה
- `src/DaatAharon.App` — ממשק Windows
- `src/DaatAharon.Core` — מודלים ולוגיקה
- `src/DaatAharon.Infrastructure` — אחסון SQLite ושירותים
- `tests/DaatAharon.Tests` — בדיקות

## עקרונות
- ברירת מחדל: חסימה עד לאישור מפורש במדיניות.
- אין להסתמך על הסתרת רכיבים בדפדפן כאמצעי אבטחה.
- כל הגדרות המשתמש נשמרות מקומית ונבדקות בטעינה.
- פרסום גרסה מותנה בבנייה ובבדיקות מוצלחות.

## פיתוח
דרוש Windows עם .NET 10 SDK. פתחו את `DaatAharon.sln` ב-Visual Studio 2026 או בגרסה תומכת .NET 10.

```powershell
dotnet restore .\DaatAharon.sln
dotnet build .\DaatAharon.sln -c Release
dotnet test .\DaatAharon.sln -c Release
```

הפרויקט עדיין בתהליך הקמה. אין להתייחס לחבילות הפצה כמאומתות עד שה-CI עובר והתקנה נבדקת על Windows נקי.
