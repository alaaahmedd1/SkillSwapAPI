using SkillSwapAPI.LoadTests;

var options = LoadOptions.Parse(args);

Console.WriteLine("SkillSwapAPI load harness");
Console.WriteLine($"  target      : {options.BaseUrl}");
Console.WriteLine($"  concurrency : {options.Concurrency} simultaneous requests per endpoint");
Console.WriteLine($"  pool size   : {options.PoolSize} seeded users");
Console.WriteLine($"  report      : {options.ReportPath}");
Console.WriteLine();

var db = new DatabaseSeeder(options.ConnectionString);
db.EnsureDatabase();
db.Migrate();

using var client = new ApiClient(options.BaseUrl);
var runner = new LoadRunner(client, options);

if (!await runner.ProbeAsync())
{
    Console.WriteLine();
    Console.WriteLine($"[load] The API is not reachable at {options.BaseUrl}.");
    Console.WriteLine("[load] Start it against the load-test profile in a separate terminal:");
    Console.WriteLine();
    Console.WriteLine("  dotnet run --project src/SkillSwapAPI.API --launch-profile LoadTest");
    Console.WriteLine();
    Console.WriteLine("[load] The LoadTest profile points at SkillSwapLoadTestDB and disables IP rate limiting.");
    return 1;
}

Console.WriteLine("[load] API is up. Seeding...");
var state = await new ApiSeeder(client, db, options).SeedAsync();

if (options.SeedOnly)
{
    Console.WriteLine("[load] --seed-only: stopping after seeding.");
    return 0;
}

Console.WriteLine("[load] Warming up...");
await runner.WarmupAsync();

var endpoints = EndpointCatalog.Build(state);
if (options.EndpointFilter is { Count: > 0 } filter)
{
    endpoints = endpoints
        .Where(endpoint => filter.Contains(endpoint.Name, StringComparer.OrdinalIgnoreCase)
                           || filter.Contains(endpoint.Group, StringComparer.OrdinalIgnoreCase))
        .ToList();
}

Console.WriteLine($"[load] Measuring {endpoints.Count} endpoints at {options.Concurrency} concurrent requests each.");
Console.WriteLine();

var results = new List<EndpointResult>(endpoints.Count);
foreach (var endpoint in endpoints)
{
    Console.Write($"[load] {endpoint.Route,-58} ");
    var result = await runner.RunAsync(endpoint, state);
    results.Add(result);

    Console.WriteLine(
        $"{result.RequestsPerSecond,8:F1} req/s | p50 {result.P50,6:F0} p95 {result.P95,6:F0} p99 {result.P99,6:F0} ms | " +
        $"2xx {result.Success,4} 4xx {result.ClientErrors,4} 5xx {result.ServerErrors,4} net {result.TransportErrors,4}");

    foreach (var sample in result.ErrorSamples.Take(1))
    {
        Console.WriteLine($"         first failure: {sample}");
    }

    await Task.Delay(250);
}

Console.WriteLine();
ReportWriter.Write(options, results, state);

var failing = results
    .Where(result => result.ServerErrors + result.TransportErrors > 0)
    .OrderByDescending(result => result.ServerErrors + result.TransportErrors)
    .ToList();

if (failing.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("[load] Endpoints that produced 5xx or transport errors:");
    foreach (var result in failing)
    {
        Console.WriteLine($"  - {result.Endpoint.Route}: 5xx {result.ServerErrors}, transport {result.TransportErrors}");
    }
}

return 0;
