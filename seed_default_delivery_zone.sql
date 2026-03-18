-- إضافة منطقة توصيل افتراضية إذا لم تكن موجودة
-- استخدم هذا السكربت عند ظهور خطأ: منطقة التوصيل غير موجودة (404)
--
-- ترميز الملف: يجب حفظ الملف كـ UTF-8.
-- عند التنفيذ بـ sqlcmd: sqlcmd -S . -d MarcketPlaceDB -f 65001 -i seed_default_delivery_zone.sql
-- (65001 = UTF-8) لتجنّب تشوه النص العربي.

IF NOT EXISTS (SELECT 1 FROM DeliveryZones WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT DeliveryZones ON;
    INSERT INTO DeliveryZones (Id, NameAr, NameEn, DeliveryFee, CreatedAt, UpdatedAt)
    VALUES (1, N'منطقة افتراضية', N'Default Zone', 8.00, SYSUTCDATETIME(), NULL);
    SET IDENTITY_INSERT DeliveryZones OFF;
    PRINT 'تم إضافة منطقة التوصيل الافتراضية بنجاح.';
END
ELSE
BEGIN
    PRINT 'منطقة التوصيل الافتراضية موجودة بالفعل.';
END
GO
