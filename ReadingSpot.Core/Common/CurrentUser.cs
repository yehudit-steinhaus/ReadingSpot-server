namespace ReadingSpot.Core.Common;

// מי שמבצעת את הבקשה. נבנה פעם אחת מה-claims ב-API ועובר ל-Services.
public record CurrentUser(int Id, bool IsAdmin);
