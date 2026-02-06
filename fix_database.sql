-- Script to fix LPMSDB database issues
-- Run this in SQL Server Management Studio (SSMS) while connected as admin

-- Step 1: Check database status
SELECT name, state_desc FROM sys.databases WHERE name = 'LPMSDB';

-- Step 2: If database is in RECOVERY_PENDING, set to EMERGENCY mode
ALTER DATABASE LPMSDB SET EMERGENCY;
GO

-- Step 3: Check for database corruption
DBCC CHECKDB ('LPMSDB') WITH NO_INFOMSGS, ALL_ERRORMSGS;
GO

-- Step 4: If corruption found, repair the database
-- Use REPAIR_ALLOW_DATA_LOSS only if necessary (this may cause data loss)
-- DBCC CHECKDB ('LPMSDB', REPAIR_ALLOW_DATA_LOSS);
-- GO

-- Step 5: Set database back to MULTI_USER mode
ALTER DATABASE LPMSDB SET MULTI_USER;
GO

-- Step 6: Set database online
ALTER DATABASE LPMSDB SET ONLINE;
GO

-- Step 7: Check final database status
SELECT name, state_desc FROM sys.databases WHERE name = 'LPMSDB';

-- Step 8: Create/fix user login mapping
USE LPMSDB;
GO

-- Check if user exists in database
SELECT name FROM sys.database_principals WHERE name = 'DEUSPC-1504GBP\user';

-- If user doesn't exist, create it
-- CREATE USER [DEUSPC-1504GBP\user] FOR LOGIN [DEUSPC-1504GBP\user];
-- GO

-- Grant necessary permissions
-- ALTER ROLE db_owner ADD MEMBER [DEUSPC-1504GBP\user];
-- GO

PRINT 'Database fix script completed';
