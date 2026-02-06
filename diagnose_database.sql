-- Comprehensive database diagnostic script
-- Run this in SSMS as sysadmin to identify the exact issue

-- Step 1: Check if SQL Server service is running and version
SELECT 
    SERVERPROPERTY('MachineName') AS MachineName,
    SERVERPROPERTY('InstanceName') AS InstanceName,
    SERVERPROPERTY('Edition') AS Edition,
    SERVERPROPERTY('ProductVersion') AS ProductVersion,
    SERVERPROPERTY('ProductLevel') AS ProductLevel;
GO

-- Step 2: Check all databases and their status
SELECT 
    name,
    state,
    state_desc,
    user_access,
    user_access_desc,
    recovery_model_desc,
    create_date
FROM sys.databases 
WHERE name = 'LPMSDB' OR name LIKE '%LPM%';
GO

-- Step 3: Check database files status
SELECT 
    name AS logical_name,
    physical_name,
    state,
    state_desc,
    size * 8.0 / 1024 AS size_mb
FROM sys.master_files
WHERE database_id = DB_ID('LPMSDB');
GO

-- Step 4: Check error log for recent issues
EXEC xp_readerrorlog 0, 1, N'LPMSDB';
GO

-- Step 5: Check if database files actually exist
EXEC ('EXEC xp_cmdshell ''dir "C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\Data\LPMSDB*"''');
GO

-- Step 6: Try to bring database online with detailed error capture
BEGIN TRY
    ALTER DATABASE LPMSDB SET ONLINE;
    PRINT 'Database successfully brought online';
END TRY
BEGIN CATCH
    PRINT 'Error bringing database online: ' + ERROR_MESSAGE();
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT 'Error Severity: ' + CAST(ERROR_SEVERITY() AS VARCHAR);
    PRINT 'Error State: ' + CAST(ERROR_STATE() AS VARCHAR);
END CATCH
GO

-- Step 7: Check current user permissions
SELECT 
    SYSTEM_USER AS current_login,
    USER AS current_user,
    IS_SRVROLEMEMBER('sysadmin') AS is_sysadmin,
    IS_MEMBER('db_owner') AS is_db_owner;
GO

PRINT 'Diagnostic completed';
