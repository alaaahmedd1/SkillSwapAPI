using System.Collections.Concurrent;
using System.Diagnostics;

namespace SkillSwapAPI.LoadTests;

public sealed record EndpointResult(
    LoadEndpoint Endpoint,
    int Total,
    int Success,
    int ClientErrors,
    int ServerErrors,
    int TransportErrors,
    double WallMs,
    double RequestsPerSecond,
    double Min,
    double Average,
    double P50,
    double P95,
    double P99,
    double Max,
    IReadOnlyDictionary<int, int> StatusCodes,
    IReadOnlyList<string> ErrorSamples);

public sealed class LoadRunner(ApiClient client, LoadOptions options)
{
    public async Task<bool> ProbeAsync()
    {
        try
        {
            var (status, _) = await client.GetAsync("/api/health");
            return status == 200;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[load] health probe failed: {ex.Message}");
            return false;
        }
    }

    public async Task WarmupAsync()
    {
        for (var round = 0; round < options.WarmupRounds; round++)
        {
            var gate = new SemaphoreSlim(64);
            var tasks = new List<Task>(options.Concurrency);
            for (var i = 0; i < options.Concurrency; i++)
            {
                await gate.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await client.GetAsync("/api/health");
                    }
                    catch
                    {
                        // warmup failures are irrelevant
                    }
                    finally
                    {
                        gate.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
            gate.Dispose();
            Console.WriteLine($"[load] warmup round {round + 1}/{options.WarmupRounds} complete.");
        }
    }

    public async Task<EndpointResult> RunAsync(LoadEndpoint endpoint, SeedState state)
    {
        var total = options.Concurrency;
        var latencies = new double[total];
        var statuses = new int[total];
        var errors = new ConcurrentBag<string>();

        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var wall = Stopwatch.StartNew();

        var tasks = new Task[total];
        for (var i = 0; i < total; i++)
        {
            var index = i;
            tasks[i] = Task.Run(async () =>
            {
                await barrier.Task;

                var request = endpoint.Factory(state, index);
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var (status, content) = await SendAsync(request);
                    stopwatch.Stop();
                    statuses[index] = status;
                    latencies[index] = stopwatch.Elapsed.TotalMilliseconds;

                    if (status >= 400 && errors.Count < 5)
                    {
                        errors.Add($"HTTP {status} {request.Method} {request.Path} -> {Truncate(content)}");
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    statuses[index] = -1;
                    latencies[index] = stopwatch.Elapsed.TotalMilliseconds;
                    if (errors.Count < 5)
                    {
                        errors.Add($"transport {request.Method} {request.Path} -> {ex.GetType().Name}: {ex.Message}");
                    }
                }
            });
        }

        barrier.SetResult();
        await Task.WhenAll(tasks);
        wall.Stop();

        return Build(endpoint, statuses, latencies, wall.Elapsed.TotalMilliseconds, errors);
    }

    private async Task<(int Status, string Content)> SendAsync(LoadRequest request)
    {
        using var message = new HttpRequestMessage(request.Method, request.Path);
        if (request.Body is not null)
        {
            message.Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request.Body),
                System.Text.Encoding.UTF8,
                "application/json");
        }

        return await client.SendAsync(message, request.Token);
    }

    private static EndpointResult Build(
        LoadEndpoint endpoint,
        int[] statuses,
        double[] latencies,
        double wallMs,
        ConcurrentBag<string> errors)
    {
        var statusCodes = new Dictionary<int, int>();
        var success = 0;
        var clientErrors = 0;
        var serverErrors = 0;
        var transportErrors = 0;

        foreach (var status in statuses)
        {
            statusCodes[status] = statusCodes.TryGetValue(status, out var count) ? count + 1 : 1;

            switch (status)
            {
                case >= 200 and < 400: success++; break;
                case >= 400 and < 500: clientErrors++; break;
                case >= 500: serverErrors++; break;
                default: transportErrors++; break;
            }
        }

        var sorted = (double[])latencies.Clone();
        Array.Sort(sorted);

        return new EndpointResult(
            endpoint,
            statuses.Length,
            success,
            clientErrors,
            serverErrors,
            transportErrors,
            wallMs,
            wallMs <= 0 ? 0 : statuses.Length / (wallMs / 1000d),
            sorted.Length == 0 ? 0 : sorted[0],
            sorted.Length == 0 ? 0 : sorted.Average(),
            Percentile(sorted, 0.50),
            Percentile(sorted, 0.95),
            Percentile(sorted, 0.99),
            sorted.Length == 0 ? 0 : sorted[^1],
            statusCodes,
            errors.OrderBy(item => item).ToList());
    }

    private static double Percentile(double[] sortedAscending, double percentile)
    {
        if (sortedAscending.Length == 0)
        {
            return 0;
        }

        var rank = percentile * (sortedAscending.Length - 1);
        var low = (int)Math.Floor(rank);
        var high = (int)Math.Ceiling(rank);
        if (low == high)
        {
            return sortedAscending[low];
        }

        return sortedAscending[low] + (sortedAscending[high] - sortedAscending[low]) * (rank - low);
    }

    private static string Truncate(string content) =>
        content.Length <= 220 ? content.Replace('\n', ' ').Replace('|', '/') : content[..220].Replace('\n', ' ').Replace('|', '/') + "...";
}
