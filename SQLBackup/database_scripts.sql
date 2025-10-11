-- ============================================
-- PhoneBook Database Scripts
-- ============================================

-- ============================================
-- 1. Create Database (Manual)
-- ============================================
USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PhoneBookDb')
BEGIN
    CREATE DATABASE PhoneBookDb;
END
GO

USE PhoneBookDb;
GO

-- ============================================
-- 2. Backup Database
-- ============================================
-- بکاپ کامل دیتابیس
BACKUP DATABASE PhoneBookDb
TO DISK = 'C:\Backup\PhoneBookDb.bak'
WITH FORMAT, 
     MEDIANAME = 'PhoneBookBackup',
     NAME = 'Full Backup of PhoneBookDb',
     COMPRESSION;
GO

-- ============================================
-- 3. Restore Database
-- ============================================
USE master;
GO

-- بستن اتصالات فعال
ALTER DATABASE PhoneBookDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- بازیابی از بکاپ
RESTORE DATABASE PhoneBookDb
FROM DISK = 'C:\Backup\PhoneBookDb.bak'
WITH REPLACE,
     RECOVERY;
GO

-- بازگشت به حالت Multi User
ALTER DATABASE PhoneBookDb SET MULTI_USER;
GO

-- ============================================
-- 4. Insert Sample Data
-- ============================================
USE PhoneBookDb;
GO

-- Sample Contacts
INSERT INTO Contacts (FullName, MobileNumber, BirthDate, CreatedAt)
VALUES 
    (N'علی احمدی', '09121234567', '1985-03-15', GETDATE()),
    (N'زهرا محمدی', '09131234567', '1990-07-22', GETDATE()),
    (N'حسین رضایی', '09141234567', '1988-11-30', GETDATE()),
    (N'فاطمه کریمی', '09151234567', '1992-05-10', GETDATE()),
    (N'محمد نوری', '09161234567', '1987-09-18', GETDATE()),
    (N'مریم صادقی', '09171234567', NULL, GETDATE()),
    (N'رضا جعفری', '09181234567', '1995-02-28', GETDATE()),
    (N'سارا حسینی', '09191234567', '1989-12-05', GETDATE()),
    (N'امیر کاظمی', '09201234567', '1991-08-14', GETDATE()),
    (N'نرگس زارعی', '09211234567', '1993-04-20', GETDATE());
GO

-- ============================================
-- 5. Useful Queries
-- ============================================

-- تعداد کل مخاطبین
SELECT COUNT(*) AS TotalContacts FROM Contacts;
GO

-- مخاطبین با تصویر
SELECT 
    c.Id,
    c.FullName,
    c.MobileNumber,
    CASE WHEN ci.Id IS NOT NULL THEN N'دارد' ELSE N'ندارد' END AS HasImage
FROM Contacts c
LEFT JOIN ContactImages ci ON c.Id = ci.ContactId;
GO

-- جستجوی مخاطبین بدون تصویر
SELECT * 
FROM Contacts c
WHERE NOT EXISTS (
    SELECT 1 FROM ContactImages ci WHERE ci.ContactId = c.Id
);
GO

-- مخاطبین با تاریخ تولد در یک بازه
SELECT *
FROM Contacts
WHERE BirthDate BETWEEN '1985-01-01' AND '1990-12-31'
ORDER BY BirthDate;
GO

-- حجم تصاویر ذخیره شده
SELECT 
    COUNT(*) AS TotalImages,
    SUM(FileSize) / 1024.0 / 1024.0 AS TotalSizeMB,
    AVG(FileSize) / 1024.0 AS AvgSizeKB
FROM ContactImages;
GO

-- ============================================
-- 6. Maintenance Queries
-- ============================================

-- حذف مخاطبین بدون تصویر (اختیاری)
-- DELETE FROM Contacts 
-- WHERE Id NOT IN (SELECT ContactId FROM ContactImages);
-- GO

-- بازسازی Index ها برای بهینه‌سازی
ALTER INDEX ALL ON Contacts REBUILD;
ALTER INDEX ALL ON ContactImages REBUILD;
GO

-- آمار دیتابیس
EXEC sp_spaceused 'Contacts';
EXEC sp_spaceused 'ContactImages';
GO

-- ============================================
-- 7. Security - Create Database User (Optional)
-- ============================================

-- ایجاد Login
-- CREATE LOGIN PhoneBookUser WITH PASSWORD = 'StrongPassword123!';
-- GO

-- ایجاد User در دیتابیس
-- USE PhoneBookDb;
-- GO
-- CREATE USER PhoneBookUser FOR LOGIN PhoneBookUser;
-- GO

-- اعطای دسترسی
-- ALTER ROLE db_datareader ADD MEMBER PhoneBookUser;
-- ALTER ROLE db_datawriter ADD MEMBER PhoneBookUser;
-- GO

-- ============================================
-- 8. Performance Monitoring
-- ============================================

-- نمایش Index های موجود
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('Contacts', 'ContactImages')
ORDER BY t.name, i.name;
GO

-- آمار Query های اجرا شده
SELECT 
    SUBSTRING(st.text, (qs.statement_start_offset/2)+1,
    ((CASE qs.statement_end_offset
        WHEN -1 THEN DATALENGTH(st.text)
        ELSE qs.statement_end_offset
    END - qs.statement_start_offset)/2) + 1) AS QueryText,
    qs.execution_count AS ExecutionCount,
    qs.total_worker_time / 1000000.0 AS TotalCPUTime_Sec,
    qs.total_elapsed_time / 1000000.0 AS TotalElapsedTime_Sec
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
WHERE st.text LIKE '%Contact%'
ORDER BY qs.execution_count DESC;
GO

-- ============================================
-- 9. Verification Queries
-- ============================================

-- بررسی Constraints
SELECT 
    tc.CONSTRAINT_NAME,
    tc.TABLE_NAME,
    tc.CONSTRAINT_TYPE,
    kcu.COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
WHERE tc.TABLE_NAME IN ('Contacts', 'ContactImages')
ORDER BY tc.TABLE_NAME, tc.CONSTRAINT_TYPE;
GO

-- بررسی Foreign Keys
SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
WHERE tp.name IN ('Contacts', 'ContactImages');
GO

-- ============================================
-- 10. Clean Up (Danger Zone!)
-- ============================================

-- حذف تمام داده‌ها (فقط برای تست)
-- DELETE FROM ContactImages;
-- DELETE FROM Contacts;
-- DBCC CHECKIDENT ('ContactImages', RESEED, 0);
-- DBCC CHECKIDENT ('Contacts', RESEED, 0);
-- GO

-- حذف دیتابیس (خطرناک!)
-- USE master;
-- GO
-- ALTER DATABASE PhoneBookDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
-- DROP DATABASE PhoneBookDb;
-- GO

-- ============================================
-- 11. Export to CSV (برای گزارش‌گیری)
-- ============================================

-- Export Contacts
-- در SSMS: Query > Results To > Results To File
-- سپس Query زیر را اجرا کنید:

SELECT 
    Id,
    FullName,
    MobileNumber,
    CONVERT(VARCHAR(10), BirthDate, 120) AS BirthDate,
    CONVERT(VARCHAR(19), CreatedAt, 120) AS CreatedAt
FROM Contacts
ORDER BY FullName;
GO

-- ============================================
-- 12. Statistics & Analytics
-- ============================================

-- تعداد مخاطبین به تفکیک ماه ایجاد
SELECT 
    YEAR(CreatedAt) AS Year,
    MONTH(CreatedAt) AS Month,
    COUNT(*) AS ContactCount
FROM Contacts
GROUP BY YEAR(CreatedAt), MONTH(CreatedAt)
ORDER BY Year DESC, Month DESC;
GO

-- رنج سنی مخاطبین
SELECT 
    COUNT(*) AS TotalWithBirthDate,
    MIN(BirthDate) AS OldestBirthDate,
    MAX(BirthDate) AS YoungestBirthDate,
    AVG(DATEDIFF(YEAR, BirthDate, GETDATE())) AS AverageAge
FROM Contacts
WHERE BirthDate IS NOT NULL;
GO

-- توزیع نوع فایل‌های تصویر
SELECT 
    ContentType,
    COUNT(*) AS Count,
    SUM(FileSize) / 1024.0 / 1024.0 AS TotalSizeMB
FROM ContactImages
GROUP BY ContentType
ORDER BY Count DESC;
GO

-- ============================================
-- 13. Health Check
-- ============================================

-- بررسی یکپارچگی دیتابیس
DBCC CHECKDB (PhoneBookDb) WITH NO_INFOMSGS;
GO

-- بررسی یکپارچگی جداول
DBCC CHECKTABLE ('Contacts') WITH NO_INFOMSGS;
DBCC CHECKTABLE ('ContactImages') WITH NO_INFOMSGS;
GO

-- آمار فضای استفاده شده
EXEC sp_spaceused @updateusage = N'TRUE';
GO

-- ============================================
-- 14. Backup Strategy
-- ============================================

-- بکاپ Differential (سریع‌تر، فقط تغییرات)
BACKUP DATABASE PhoneBookDb
TO DISK = 'C:\Backup\PhoneBookDb_Diff.bak'
WITH DIFFERENTIAL,
     COMPRESSION;
GO

-- بکاپ Transaction Log
BACKUP LOG PhoneBookDb
TO DISK = 'C:\Backup\PhoneBookDb_Log.trn'
WITH COMPRESSION;
GO

-- ============================================
-- Notes برای DBA
-- ============================================

/*
1. بکاپ روزانه:
   - Full Backup هفتگی
   - Differential Backup روزانه
   - Transaction Log Backup هر ساعت

2. نگهداری:
   - Rebuild Index ها هفتگی
   - Update Statistics هفتگی
   - DBCC CHECKDB ماهانه

3. امنیت:
   - استفاده از Least Privilege برای User ها
   - رمزگذاری Connection String
   - فعال کردن SQL Server Audit

4. بهینه‌سازی:
   - بررسی Execution Plan های کند
   - Monitoring Query Performance
   - تنظیم Memory و CPU

5. Disaster Recovery:
   - نگهداری بکاپ در مکان جداگانه
   - تست Restore به صورت دوره‌ای
   - مستندسازی Recovery Procedures
*/