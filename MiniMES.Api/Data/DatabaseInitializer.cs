using Microsoft.EntityFrameworkCore;

namespace MiniMES.Api.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureCreatedAsync(MiniMesDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        await EnsureProcessesTableAsync(dbContext);
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
}
