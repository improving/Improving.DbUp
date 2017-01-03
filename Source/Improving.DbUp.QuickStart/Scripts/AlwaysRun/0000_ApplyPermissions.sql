IF '$AppUser$' <> 'UNDEFINED'
BEGIN
    IF NOT EXISTS (select * from [master].[dbo].[syslogins] WHERE [name] = '$AppUser$')
    BEGIN
        CREATE LOGIN [$AppUser$] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
    END

    IF NOT EXISTS (SELECT * FROM [$DbName$].[sys].[database_principals] WHERE [name] = N'$AppUser$')
    BEGIN
        USE [$DbName$] 
        CREATE USER [$AppUser$] FOR LOGIN [$AppUser$] WITH DEFAULT_SCHEMA=[dbo]
        EXEC sp_addrolemember N'db_owner', N'$AppUser$'
    END

    IF EXISTS (SELECT * FROM [master].[dbo].[sysdatabases] WHERE [name] = '$DbName$')
        AND NOT EXISTS (SELECT * FROM [$DbName$].[sys].[database_principals] WHERE [name] = N'$AppUser$')
    BEGIN
        USE [$DbName$] 
        CREATE USER [$AppUser$] FOR LOGIN [$AppUser$] WITH DEFAULT_SCHEMA=[dbo]
        EXEC sp_addrolemember N'db_owner', N'$AppUser$'
    END

END
GO

IF '$Env$' = 'DEV'
BEGIN
    IF NOT EXISTS (select * from [master].[dbo].[syslogins] WHERE [name] = '$TestUser$')
    BEGIN
        CREATE LOGIN [$TestUser$] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
    END

    IF NOT EXISTS (SELECT * FROM [$DbName$].[sys].[database_principals] WHERE [name] = N'$TestUser$')
    BEGIN
        USE [$DbName$] 
        CREATE USER [$TestUser$] FOR LOGIN [$TestUser$] WITH DEFAULT_SCHEMA=[dbo]
        EXEC sp_addrolemember N'db_owner', N'$TestUser$'
    END

    IF EXISTS (SELECT * FROM [master].[dbo].[sysdatabases] WHERE [name] = '$DbName$')
        AND NOT EXISTS (SELECT * FROM [$DbName$].[sys].[database_principals] WHERE [name] = N'$TestUser$')
    BEGIN
        USE [$DbName$] 
        CREATE USER [$TestUser$] FOR LOGIN [$TestUser$] WITH DEFAULT_SCHEMA=[dbo]
        EXEC sp_addrolemember N'db_owner', N'$TestUser$'
    END

END
GO