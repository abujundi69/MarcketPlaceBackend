-- إضافة إعدادات النظام الافتراضية إذا لم تكن موجودة
-- استخدم عند ظهور خطأ: إعدادات النظام غير موجودة (400)
--
-- ترميز الملف: يجب حفظ الملف كـ UTF-8.
-- عند التنفيذ بـ sqlcmd: sqlcmd -S . -d MarcketPlaceDB -f 65001 -i seed_default_system_settings.sql
-- (65001 = UTF-8) لتجنّب تشوه النص العربي.

IF NOT EXISTS (SELECT 1 FROM SystemSettings)
BEGIN
    INSERT INTO SystemSettings (
        SystemNameAr,
        SystemNameEn,
        FooterAr,
        FooterEn,
        CustomerPromoMessage,
        Logo,
        PickupNameAr,
        PickupNameEn,
        PickupAddressText,
        PickupLatitude,
        PickupLongitude,
        UpdatedAt
    )
    VALUES (
        N'زاد',
        N'Zad',
        N'© زاد - جميع الحقوق محفوظة',
        N'© Zad - All rights reserved',
        NULL,
        NULL,
        N'المستودع الرئيسي',
        N'Main Warehouse',
        N'الموقع الافتراضي',
        31.5,
        34.5,
        SYSUTCDATETIME()
    );
    PRINT 'تم إضافة إعدادات النظام الافتراضية بنجاح.';
END
ELSE
BEGIN
    PRINT 'إعدادات النظام موجودة بالفعل.';
END
GO
