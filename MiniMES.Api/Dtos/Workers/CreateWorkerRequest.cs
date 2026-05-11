namespace MiniMES.Api.Dtos.Workers;

public class CreateWorkerRequest
{
    public string WorkerCode { get; set; } = string.Empty;

    public string WorkerName { get; set; } = string.Empty;

    public string? Department { get; set; }

    public string? Role { get; set; }

    public string? ShiftGroup { get; set; }
}
