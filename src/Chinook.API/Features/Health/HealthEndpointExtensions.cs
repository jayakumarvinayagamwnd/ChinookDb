using Chinook.API.Features.Health.Formatting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Chinook.API.Features.Health;

public static class HealthEndpointExtensions
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

                app.MapGet("/health-ui-api", async (
                        HttpContext context,
                        HealthCheckService healthCheckService,
                        CancellationToken cancellationToken) =>
                {
                        var report = await healthCheckService.CheckHealthAsync(_ => true, cancellationToken);
                        context.Response.StatusCode = report.Status == HealthStatus.Unhealthy
                                ? StatusCodes.Status503ServiceUnavailable
                                : StatusCodes.Status200OK;

                        await HealthCheckResponseWriter.WriteResponse(context, report);
                });

                app.MapGet("/health-ui", () => Results.Content(GetDashboardHtml(), "text/html; charset=utf-8"));

        return app;
    }

        private static string GetDashboardHtml() => """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Chinook Health Dashboard</title>
    <style>
        :root {
            --bg: #f6f8fc;
            --surface: #ffffff;
            --ink: #1d2433;
            --muted: #6f7787;
            --ok: #1f9d5a;
            --warn: #d19000;
            --bad: #cc2936;
            --accent: #0078d4;
            --line: #e7ebf3;
            --shadow: 0 8px 30px rgba(23, 30, 53, 0.08);
        }

        * { box-sizing: border-box; }
        body {
            margin: 0;
            background: radial-gradient(circle at top right, #dceafc 0%, var(--bg) 46%);
            color: var(--ink);
            font-family: "Segoe UI", "Helvetica Neue", Tahoma, sans-serif;
            min-height: 100vh;
        }

        .shell {
            max-width: 1100px;
            margin: 0 auto;
            padding: 28px 18px 40px;
        }

        .hero {
            background: linear-gradient(115deg, #0f365d 0%, #09589d 52%, #1d7ac7 100%);
            color: white;
            border-radius: 18px;
            padding: 24px 26px;
            box-shadow: var(--shadow);
            margin-bottom: 18px;
        }

        .hero h1 {
            margin: 0;
            font-size: 1.6rem;
            letter-spacing: 0.2px;
        }

        .hero p {
            margin: 8px 0 0;
            color: #d7e8fb;
            font-size: 0.95rem;
        }

        .grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
            gap: 14px;
            margin-bottom: 16px;
        }

        .stat {
            background: var(--surface);
            border: 1px solid var(--line);
            border-radius: 14px;
            padding: 14px;
            box-shadow: var(--shadow);
        }

        .label { color: var(--muted); font-size: 0.84rem; }
        .value { margin-top: 7px; font-size: 1.15rem; font-weight: 700; }

        table {
            width: 100%;
            border-collapse: collapse;
            background: var(--surface);
            border-radius: 14px;
            overflow: hidden;
            border: 1px solid var(--line);
            box-shadow: var(--shadow);
        }

        th, td {
            padding: 12px 13px;
            border-bottom: 1px solid var(--line);
            text-align: left;
            font-size: 0.93rem;
            vertical-align: top;
        }

        th {
            color: var(--muted);
            font-weight: 600;
            background: #f9fbff;
        }

        tr:last-child td { border-bottom: none; }

        .badge {
            display: inline-block;
            border-radius: 999px;
            padding: 3px 10px;
            font-size: 0.78rem;
            font-weight: 700;
            letter-spacing: .2px;
            text-transform: uppercase;
        }

        .healthy { background: #e7f7ee; color: var(--ok); }
        .degraded { background: #fff8e7; color: var(--warn); }
        .unhealthy { background: #fdebed; color: var(--bad); }

        .mono { font-family: Consolas, "Courier New", monospace; }

        .toolbar {
            display: flex;
            align-items: center;
            gap: 10px;
            margin: 12px 0 14px;
            flex-wrap: wrap;
        }

        button {
            border: 0;
            background: var(--accent);
            color: #fff;
            border-radius: 8px;
            padding: 8px 12px;
            font-weight: 600;
            cursor: pointer;
        }

        .note { color: var(--muted); font-size: 0.86rem; }
    </style>
</head>
<body>
    <div class="shell">
        <div class="hero">
            <h1>Chinook API Health Dashboard</h1>
            <p>Live operational status for all registered health probes.</p>
        </div>

        <div class="grid">
            <div class="stat"><div class="label">Overall Status</div><div id="overall" class="value">Loading...</div></div>
            <div class="stat"><div class="label">Checks</div><div id="count" class="value">-</div></div>
            <div class="stat"><div class="label">Total Duration</div><div id="duration" class="value">-</div></div>
            <div class="stat"><div class="label">Last Refresh (UTC)</div><div id="last" class="value">-</div></div>
        </div>

        <div class="toolbar">
            <button id="refresh">Refresh now</button>
            <span class="note">Auto-refresh every 15 seconds.</span>
        </div>

        <table>
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Status</th>
                    <th>Duration</th>
                    <th>Description / Error</th>
                    <th>Tags</th>
                </tr>
            </thead>
            <tbody id="rows">
                <tr><td colspan="5">Loading health data...</td></tr>
            </tbody>
        </table>
    </div>

    <script>
        const ui = {
            overall: document.getElementById('overall'),
            count: document.getElementById('count'),
            duration: document.getElementById('duration'),
            last: document.getElementById('last'),
            rows: document.getElementById('rows'),
            refresh: document.getElementById('refresh')
        };

        function badge(status) {
            const lower = (status || '').toLowerCase();
            const cls = lower === 'healthy' ? 'healthy' : lower === 'degraded' ? 'degraded' : 'unhealthy';
            return `<span class="badge ${cls}">${status}</span>`;
        }

        async function loadHealth() {
            try {
                const response = await fetch('/health-ui-api', { cache: 'no-store' });
                const payload = await response.json();

                ui.overall.innerHTML = badge(payload.status || 'Unknown');
                ui.count.textContent = String(payload.entries?.length || 0);
                ui.duration.textContent = `${(payload.totalDurationMs ?? 0).toFixed(2)} ms`;
                ui.last.textContent = payload.generatedAtUtc || '-';

                const rows = (payload.entries || []).map(entry => `
                    <tr>
                        <td class="mono">${entry.name}</td>
                        <td>${badge(entry.status)}</td>
                        <td>${(entry.durationMs ?? 0).toFixed(2)} ms</td>
                        <td>${entry.description || entry.exception || '-'}</td>
                        <td>${(entry.tags || []).join(', ') || '-'}</td>
                    </tr>`).join('');

                ui.rows.innerHTML = rows || '<tr><td colspan="5">No health check entries were returned.</td></tr>';
            } catch (err) {
                ui.overall.innerHTML = badge('Unhealthy');
                ui.rows.innerHTML = `<tr><td colspan="5">Failed to load /health-ui-api: ${String(err)}</td></tr>`;
            }
        }

        ui.refresh.addEventListener('click', loadHealth);
        loadHealth();
        setInterval(loadHealth, 15000);
    </script>
</body>
</html>
""";
}
