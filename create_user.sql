-- Script to create and configure user login for LPMSDB
-- Run this in SSMS connected as sysadmin

-- Step 1: Create server login if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'DEUSPC-1504GBP\user')
BEGIN
    CREATE LOGIN [DEUSPC-1504GBP\user] FROM WINDOWS;
    PRINT 'Server login created for DEUSPC-1504GBP\user';
END
ELSE
BEGIN
    PRINT 'Server login already exists for DEUSPC-1504GBP\user';
END
GO

-- Step 2: Use the LPMSDB database
USE LPMSDB;
GO

-- Step 3: Create database user if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'DEUSPC-1504GBP\user')
BEGIN
    CREATE USER [DEUSPC-1504GBP\user] FOR LOGIN [DEUSPC-1504GBP\user];
    PRINT 'Database user created for DEUSPC-1504GBP\user';
END
ELSE
BEGIN
    PRINT 'Database user already exists for DEUSPC-1504GBP\user';
END
GO

-- Step 4: Grant db_owner role (full permissions)
ALTER ROLE db_owner ADD MEMBER [DEUSPC-1504GBP\user];
PRINT 'Granted db_owner role to DEUSPC-1504GBP\user';
GO

-- Step 5: Verify permissions
SELECT 
    dp.name AS database_user,
    dp.type_desc AS user_type,
    rm.name AS role_name
FROM sys.database_principals dp
JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id
JOIN sys.database_principals rm ON drm.role_principal_id = rm.principal_id
WHERE dp.name = 'DEUSPC-1504GBP\user';
GO

PRINT 'User configuration completed successfully';
