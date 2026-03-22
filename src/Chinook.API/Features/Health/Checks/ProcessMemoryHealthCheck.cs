using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Chinook.API.Features.Health.Checks;

public sealed class ProcessMemoryHealthCheck : IHealthCheck
{
    private const long MaxWorkingSetBytes = 1_024L * 1_024L * 1_024L;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var process = Environment.ProcessId;
        using var current = System.Diagnostics.Process.GetCurrentProcess();

        var workingSet = current.WorkingSet64;
        var privateMemory = current.PrivateMemorySize64;

        var data = new Dictionary<string, object>
        {
            ["processId"] = process,
            ["workingSetBytes"] = workingSet,
            ["privateMemoryBytes"] = privateMemory,
            ["thresholdBytes"] = MaxWorkingSetBytes
        };

        if (workingSet > MaxWorkingSetBytes)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                "Process memory is above threshold.",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            "Process memory is within threshold.",
            data));
    }
}
