-- حذف سجل الـ migration لأنها طُبقت فارغة (بدون إنشاء الجدول)
-- شغّل هذا السكربت على قاعدة البيانات ثم نفّذ: dotnet ef database update

DELETE FROM [__EFMigrationsHistory] 
WHERE MigrationId = '20260312110319_AddCustomerFavorites';
