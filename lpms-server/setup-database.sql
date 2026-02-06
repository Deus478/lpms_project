-- Create LPMSDB database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'LPMSDB')
BEGIN
    CREATE DATABASE LPMSDB;
    PRINT 'Database LPMSDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database LPMSDB already exists.';
END
GO
