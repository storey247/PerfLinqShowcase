using BenchmarkDotNet.Running;
using PerfLinqShowcase.Benchmarks;
using PerfLinqShowcase.DataGeneration;

// ─── Quick sanity check (non-benchmark mode) ─────────────────────────────────
if (args.Length > 0 && args[0] == "--preview")
{
    RunPreview();
    return;
}

// ─── BenchmarkDotNet requires Release build ───────────────────────────────────
#if DEBUG
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("⚠  Running in DEBUG mode — results are not representative.");
Console.WriteLine("   Build in Release to get accurate benchmark numbers.");
Console.WriteLine("   dotnet run -c Release -- [filter]");
Console.ResetColor();
Console.WriteLine();
#endif

// Run the suite selected by the first argument, or all if none given.
// Examples:
//   dotnet run -c Release                  → all benchmarks
//   dotnet run -c Release -- Bench1        → only Bench1
//   dotnet run -c Release -- --preview     → quick data description, no benchmark

var filter = args.Length > 0 ? args[0] : "*";

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new BenchmarkConfig());
return;

// ─── Preview mode ─────────────────────────────────────────────────────────────
static void RunPreview()
{
    Console.WriteLine("=== PerfLinqShowcase — Dataset Preview ===");
    Console.WriteLine();

    var scales = new (string label, int c, int d, int e, int p, int t)[]
    {
        ("Bench1/2 Small",  2,  3, 10,  2,  5),
        ("Bench1/2 Medium", 5,  4, 20,  3, 10),
        ("Bench1/2 Large",  10, 5, 40,  4, 20),
        ("Bench3 100e",     1,  1, 100, 5, 15),
        ("Bench3 1000e",    1,  1, 1_000, 5, 15),
        ("Bench3 10000e",   1,  1, 10_000, 5, 15),
        ("Bench4 Small",    3,  3, 15,  3,  8),
        ("Bench4 Medium",   5,  5, 30,  4, 15),
        ("Bench4 Large",    10, 8, 50,  5, 20),
        ("Bench5 Small",    2,  3, 20,  2,  5),
        ("Bench5 Medium",   5,  5, 40,  3, 10),
        ("Bench5 Large",    10, 8, 80,  4, 15),
    };

    foreach (var (label, c, d, e, p, t) in scales)
    {
        var data = DataGenerator.Generate(c, d, e, p, t);
        Console.WriteLine($"  {label,-22} → {DataGenerator.Describe(data)}");
    }

    Console.WriteLine();
    Console.WriteLine("Run without --preview to execute the full benchmarks.");
}
