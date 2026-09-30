namespace SkillSwapAPI.LoadTests;

public sealed record LoadOptions(
    string BaseUrl,
    string ConnectionString,
    int Concurrency,
    int WarmupRounds,
    int PoolSize,
    string ReportPath,
    IReadOnlyList<string>? EndpointFilter,
    bool SeedOnly)
{
    public static LoadOptions Parse(string[] args)
    {
        string baseUrl = "http://localhost:5099";
        string connectionString = "Server=.;Database=SkillSwapLoadTestDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        int concurrency = 1000;
        int warmup = 2;
        int poolSize = 1000;
        string reportPath = "load-report.md";
        IReadOnlyList<string>? filter = null;
        bool seedOnly = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--base-url": baseUrl = args[++i]; break;
                case "--connection": connectionString = args[++i]; break;
                case "--concurrency": concurrency = int.Parse(args[++i]); break;
                case "--warmup": warmup = int.Parse(args[++i]); break;
                case "--pool-size": poolSize = int.Parse(args[++i]); break;
                case "--report": reportPath = args[++i]; break;
                case "--endpoints": filter = args[++i].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries); break;
                case "--seed-only": seedOnly = true; break;
                default: throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        return new LoadOptions(baseUrl, connectionString, concurrency, warmup, poolSize, reportPath, filter, seedOnly);
    }
}
