-- إضافة عمود ProductImage لجدول OrderItems إذا لم يكن موجوداً
-- استخدم هذا السكربت عند ظهور خطأ: Invalid column name 'ProductImage'

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('OrderItems') AND name = 'ProductImage'
)
BEGIN
    ALTER TABLE OrderItems ADD ProductImage varbinary(max) NULL;
    PRINT 'تم إضافة عمود ProductImage بنجاح.';
END
ELSE
BEGIN
    PRINT 'عمود ProductImage موجود بالفعل.';
END
GO
