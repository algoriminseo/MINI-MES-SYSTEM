using Microsoft.EntityFrameworkCore;

namespace MiniMES.Api.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureCreatedAsync(MiniMesDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        await EnsureProcessesTableAsync(dbContext);
        await EnsureEquipmentsTableAsync(dbContext);
        await EnsureWorkersTableAsync(dbContext);
        await EnsureWorkOrdersTableAsync(dbContext);
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

    private static async Task EnsureWorkersTableAsync(MiniMesDbContext dbContext)
    {
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Workers]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Workers] (
                    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Workers] PRIMARY KEY,
                    [WorkerCode] NVARCHAR(50) NOT NULL,
                    [WorkerName] NVARCHAR(100) NOT NULL,
                    [Department] NVARCHAR(100) NULL,
                    [Role] NVARCHAR(50) NULL,
                    [ShiftGroup] NVARCHAR(30) NULL,
                    [IsActive] BIT NOT NULL,
                    [CreatedAt] DATETIME2 NOT NULL,
                    [UpdatedAt] DATETIME2 NULL
                );

                CREATE UNIQUE INDEX [IX_Workers_WorkerCode]
                    ON [dbo].[Workers] ([WorkerCode]);
            END
            """);
    }

    private static async Task EnsureWorkOrdersTableAsync(MiniMesDbContext dbContext)
    {
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[WorkOrders]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[WorkOrders] (
                    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WorkOrders] PRIMARY KEY,
                    [WorkOrderNo] NVARCHAR(50) NOT NULL,
                    [ItemId] INT NOT NULL,
                    [OrderQuantity] INT NOT NULL,
                    [ProducedQuantity] INT NOT NULL,
                    [DefectQuantity] INT NOT NULL,
                    [Status] NVARCHAR(30) NOT NULL,
                    [PlannedStartDate] DATETIME2 NULL,
                    [DueDate] DATETIME2 NULL,
                    [StartedAt] DATETIME2 NULL,
                    [CompletedAt] DATETIME2 NULL,
                    [Remark] NVARCHAR(500) NULL,
                    [IsActive] BIT NOT NULL,
                    [CreatedAt] DATETIME2 NOT NULL,
                    [UpdatedAt] DATETIME2 NULL,
                    CONSTRAINT [FK_WorkOrders_Items_ItemId]
                        FOREIGN KEY ([ItemId]) REFERENCES [dbo].[Items] ([Id])
                );

                CREATE UNIQUE INDEX [IX_WorkOrders_WorkOrderNo]
                    ON [dbo].[WorkOrders] ([WorkOrderNo]);

                CREATE INDEX [IX_WorkOrders_ItemId]
                    ON [dbo].[WorkOrders] ([ItemId]);
            END
            """);
    }
}
