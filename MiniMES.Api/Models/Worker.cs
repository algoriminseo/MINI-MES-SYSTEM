namespace MiniMES.Api.Models;

public class Worker
{
    public int Id { get; set; }

    public string WorkerCode { get; set; } = string.Empty;

    public string WorkerName { get; set; } = string.Empty;

    public string? Department { get; set; }

    public string? Role { get; set; }

    public string? ShiftGroup { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
