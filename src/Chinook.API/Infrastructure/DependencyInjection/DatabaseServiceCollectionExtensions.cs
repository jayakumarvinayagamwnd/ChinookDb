using Chinook.API.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Chinook.API.Infrastructure.DependencyInjection;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddChinookDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ChinookDb")
            ?? configuration["ChinookDbConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string for ChinookDb is not configured.");

        connectionString = ResolveSqliteConnectionString(connectionString);
        Log.Information("Using connection string: {ConnectionString}", connectionString);

        services.AddDbContext<ChinookDbContext>(options => options.UseSqlite(connectionString));

        return services;
    }

    private static string ResolveSqliteConnectionString(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;

        if (string.IsNullOrEmpty(dataSource) || Path.IsPathRooted(dataSource))
            return connectionString;

        var resolvedPath = FindDbFileInAncestors(dataSource);
        if (resolvedPath is null)
            return connectionString;

        builder.DataSource = resolvedPath;
        return builder.ToString();
    }

    private static string? FindDbFileInAncestors(string relativePath)
    {
        foreach (var basePath in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var current = new DirectoryInfo(basePath);
            while (current is not null)
            {
                var candidate = Path.Combine(current.FullName, relativePath);
                if (File.Exists(candidate))
                    return candidate;

                current = current.Parent;
            }
        }

        return null;
    }
}