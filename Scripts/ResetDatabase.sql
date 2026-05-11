/*
    Mini MES database reset script.

    WARNING:
    This drops the whole MiniMesDb database and all data inside it.
    After running this script, start the API again with `dotnet run`.
    The API startup code will recreate the database and current tables.
*/

USE master;
GO

IF DB_ID(N'MiniMesDb') IS NOT NULL
BEGIN
    ALTER DATABASE MiniMesDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MiniMesDb;
END
GO
