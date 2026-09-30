using System.Text;

namespace SkillSwapAPI.LoadTests;

public static class ReportWriter
{
    public static void Write(LoadOptions options, IReadOnlyList<EndpointResult> results, SeedState state)
    {
        var builder = new StringBuilder();

        builder.AppendLine("# SkillSwapAPI load report");
        builder.AppendLine();
        builder.AppendLine($"- Target: `{options.BaseUrl}`");
        builder.AppendLine($"- Database: `{Redact(options.ConnectionString)}`");
        builder.AppendLine($"- Generated (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"- Seed run stamp: `{state.RunStamp}`");
        builder.AppendLine($"- **Concurrent requests per endpoint: {options.Concurrency}** (fired simultaneously, no ramp)");
        builder.AppendLine($"- Endpoints measured: {results.Count}");
        builder.AppendLine();

        builder.AppendLine("## How to read this");
        builder.AppendLine();
        builder.AppendLine("Every endpoint was hit with the full concurrency in one burst. `Wall` is the time from the");
        builder.AppendLine("first request leaving the harness until the last response arrived; `Req/s` is");
        builder.AppendLine("`concurrency / wall`. Latency percentiles are measured client side and therefore include");
        builder.AppendLine("queueing inside Kestrel and the SQL connection pool, which is what a real client would see.");
        builder.AppendLine();
        builder.AppendLine("`2xx` counts every non-error response, `4xx`/`5xx` are HTTP status buckets and `net` counts");
        builder.AppendLine("requests that never produced a response (timeout, socket reset, refused connection).");
        builder.AppendLine();

        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("| Endpoint | Route | Wall (ms) | Req/s | p50 | p95 | p99 | Max | 2xx | 4xx | 5xx | net |");
        builder.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|");

        foreach (var result in results.OrderByDescending(item => item.RequestsPerSecond))
        {
            builder.AppendLine(
                $"| {result.Endpoint.Name} | `{result.Endpoint.Route}` | {result.WallMs:F0} | {result.RequestsPerSecond:F1} | " +
                $"{result.P50:F0} | {result.P95:F0} | {result.P99:F0} | {result.Max:F0} | " +
                $"{result.Success} | {result.ClientErrors} | {result.ServerErrors} | {result.TransportErrors} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Slowest endpoints by p95");
        builder.AppendLine();
        builder.AppendLine("| Endpoint | Route | p95 (ms) | p99 (ms) | Req/s |");
        builder.AppendLine("|---|---|---:|---:|---:|");
        foreach (var result in results.OrderByDescending(item => item.P95).Take(10))
        {
            builder.AppendLine(
                $"| {result.Endpoint.Name} | `{result.Endpoint.Route}` | {result.P95:F0} | {result.P99:F0} | {result.RequestsPerSecond:F1} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Fastest endpoints by p95");
        builder.AppendLine();
        builder.AppendLine("| Endpoint | Route | p95 (ms) | Req/s |");
        builder.AppendLine("|---|---|---:|---:|");
        foreach (var result in results.OrderBy(item => item.P95).Take(10))
        {
            builder.AppendLine(
                $"| {result.Endpoint.Name} | `{result.Endpoint.Route}` | {result.P95:F0} | {result.RequestsPerSecond:F1} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Per-endpoint detail");

        foreach (var group in results.GroupBy(item => item.Endpoint.Group).OrderBy(item => item.Key))
        {
            builder.AppendLine();
            builder.AppendLine($"### {group.Key}");

            foreach (var result in group)
            {
                builder.AppendLine();
                builder.AppendLine($"#### `{result.Endpoint.Route}` — {result.Endpoint.Name}");
                builder.AppendLine();
                builder.AppendLine($"- Kind: {(result.Endpoint.Mutating ? "mutating" : "read-only")}");
                if (!string.IsNullOrEmpty(result.Endpoint.Notes))
                {
                    builder.AppendLine($"- Notes: {result.Endpoint.Notes}");
                }

                builder.AppendLine($"- Throughput: **{result.RequestsPerSecond:F1} req/s** over {result.WallMs:F0} ms for {result.Total} concurrent requests");
                builder.AppendLine($"- Latency (ms): min {result.Min:F0} / avg {result.Average:F0} / p50 {result.P50:F0} / p95 {result.P95:F0} / p99 {result.P99:F0} / max {result.Max:F0}");
                builder.AppendLine($"- Outcomes: 2xx {result.Success}, 4xx {result.ClientErrors}, 5xx {result.ServerErrors}, transport {result.TransportErrors}");
                builder.AppendLine($"- Status codes: {FormatStatuses(result.StatusCodes)}");

                if (result.ErrorSamples.Count > 0)
                {
                    builder.AppendLine("- Sample failures:");
                    foreach (var sample in result.ErrorSamples)
                    {
                        builder.AppendLine($"  - `{sample}`");
                    }
                }
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(options.ReportPath))!);
        File.WriteAllText(options.ReportPath, builder.ToString());
        Console.WriteLine($"[load] Report written to {Path.GetFullPath(options.ReportPath)}");
    }

    private static string FormatStatuses(IReadOnlyDictionary<int, int> statusCodes) =>
        string.Join(
            ", ",
            statusCodes
                .OrderByDescending(pair => pair.Value)
                .Select(pair => $"{(pair.Key < 0 ? "transport" : pair.Key.ToString())}×{pair.Value}"));

    private static string Redact(string connectionString)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(';', parts.Select(part =>
            part.StartsWith("Password", StringComparison.OrdinalIgnoreCase)
                ? "Password=***"
                : part));
    }
}
