using Microsoft.EntityFrameworkCore;

namespace MiniMES.Api.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureCreatedAsync(MiniMesDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        await EnsureProcessesTableAsync(dbContext);
        await EnsureEquipmentsTableAsync(dbContext);
    }

    private static async Task EnsureProcessesTableAsync(MiniMesDbContext dbContext)
    {
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Processes]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Processes] (
                    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Processes] PRIMARY KEY,
                    [ProcessCode] NVARCHAR(50) NOT NULL,
                    [ProcessName] NVARCHAR(100) NOT NULL,
                    [Description] NVARCHAR(300) NULL,
                    [SortOrder] INT NOT NULL,
                    [IsInspection] BIT NOT NULL,
                    [IsActive] BIT NOT NULL,
                    [CreatedAt] DATETIME2 NOT NULL,
                    [UpdatedAt] DATETIME2 NULL
                );

                CREATE UNIQUE INDEX [IX_Processes_ProcessCode]
                    ON [dbo].[Processes] ([ProcessCode]);
            END
            """);
    }

    private static async Task EnsureEquipmentsTableAsync(MiniMesDbContext dbContext)
    {
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Equipments]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Equipments] (
                    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Equipments] PRIMARY KEY,
                    [EquipmentCode] NVARCHAR(50) NOT NULL,
                    [EquipmentName] NVARCHAR(100) NOT NULL,
                    [ProcessId] INT NOT NULL,
                    [Status] NVARCHAR(30) NOT NULL,
                    [Location] NVARCHAR(100) NULL,
                    [Description] NVARCHAR(300) NULL,
                    [IsActive] BIT NOT NULL,
                    [CreatedAt] DATETIME2 NOT NULL,
                    [UpdatedAt] DATETIME2 NULL,
                    CONSTRAINT [FK_Equipments_Processes_ProcessId]
                        FOREIGN KEY ([ProcessId]) REFERENCES [dbo].[Processes] ([Id])
                );

                CREATE UNIQUE INDEX [IX_Equipments_EquipmentCode]
                    ON [dbo].[Equipments] ([EquipmentCode]);

                CREATE INDEX [IX_Equipments_ProcessId]
                    ON [dbo].[Equipments] ([ProcessId]);
            END
            """);
    }
}
