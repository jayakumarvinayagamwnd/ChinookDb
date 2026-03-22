using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Chinook.API.Features.Health.Checks;

public sealed class SqliteFileHealthCheck(IConfiguration configuration, IWebHostEnvironment environment)
    : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("ChinookDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Connection string 'ChinookDb' is missing."));
        }

        var dataSource = GetSqliteDataSourcePath(connectionString);
        if (string.IsNullOrWhiteSpace(dataSource))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Unable to parse SQLite data source from connection string."));
        }

        var absolutePath = Path.IsPathRooted(dataSource)
            ? dataSource
            : Path.Combine(environment.ContentRootPath, dataSource);

        if (!File.Exists(absolutePath))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"SQLite database file was not found at '{absolutePath}'."));
        }

        var fileInfo = new FileInfo(absolutePath);
        var data = new Dictionary<string, object>
        {
            ["path"] = absolutePath,
            ["sizeBytes"] = fileInfo.Length,
            ["lastWriteUtc"] = fileInfo.LastWriteTimeUtc
        };

        return Task.FromResult(HealthCheckResult.Healthy(
            "SQLite database file is present.",
            data));
    }

    private static string? GetSqliteDataSourcePath(string connectionString)
    {
        const string key = "Data Source=";
        var index = connectionString.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var value = connectionString[(index + key.Length)..];
        var semicolonIndex = value.IndexOf(';');
        if (semicolonIndex >= 0)
        {
            value = value[..semicolonIndex];
        }

        return value.Trim();
    }
}
