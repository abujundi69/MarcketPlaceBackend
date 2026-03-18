-- إضافة عمود Image لجدول ProductRequests إذا لم يكن موجوداً
-- استخدم هذا السكربت إذا ظهر خطأ: Invalid column name 'Image'

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('ProductRequests') AND name = 'Image'
)
BEGIN
    ALTER TABLE ProductRequests ADD Image varbinary(max) NULL;
    PRINT 'تم إضافة عمود Image بنجاح.';
END
ELSE
BEGIN
    PRINT 'عمود Image موجود بالفعل.';
END
GO
