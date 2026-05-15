using BenchmarkDotNet.Attributes;
using PerfLinqShowcase.DataGeneration;
using PerfLinqShowcase.Models;

namespace PerfLinqShowcase.Benchmarks;

/// <summary>
/// BENCHMARK 5 — GroupBy + aggregation + sort pipeline
///
/// Task: Produce a salary-band report per department across all companies:
///       band ("Junior" / "Mid" / "Senior"), employee count, average salary,
///       total billable hours — sorted by department name then band.
///
/// Demonstrates:
///   • GroupBy materialises a lookup structure (Dictionary of lists) internally,
///     buffering all matching elements before the first result is produced.
///   • Chaining OrderBy after GroupBy → Select forces a second full sort over
///     the projected results.
///   • The LINQ pipeline produces ~N intermediate objects:
///       SelectMany results, Grouping&lt;K,V&gt; instances, anonymous projection types,
///       the sorted array backing OrderBy.
///   • The foreach version uses a plain Dictionary and accumulates in a single
///     pass with zero intermediate collections.
///   • At large scale, GroupBy + OrderBy is one of the highest-allocation LINQ
///     combinations because it cannot stream — everything must be buffered.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class Bench5_GroupByAggregation
{
    private List<Company> _companies = null!;

    // Scale: companies × depts × employees × projects × timeEntries
    //   Small  →  2×3×20×2×5   = 1,200 time entries
    //   Medium →  5×5×40×3×10  = 30,000 time entries
    //   Large  → 10×8×80×4×15  = 384,000 time entries
    [Params("Small", "Medium", "Large")]
    public string Scale = "Small";

    [GlobalSetup]
    public void Setup()
    {
        _companies = Scale switch
        {
            "Small"  => DataGenerator.Generate(2,  3, 20, 2,  5),
            "Medium" => DataGenerator.Generate(5,  5, 40, 3, 10),
            "Large"  => DataGenerator.Generate(10, 8, 80, 4, 15),
            _        => throw new ArgumentOutOfRangeException(nameof(Scale))
        };
    }

    // ─── LINQ versions ────────────────────────────────────────────────────────

    /// <summary>
    /// Full LINQ pipeline with SelectMany → Where → GroupBy → Select → OrderBy.
    ///
    /// Allocations:
    ///   • SelectMany(×2) iterator objects
    ///   • WhereIterator wrapper
    ///   • GroupBy internal Lookup&lt;K, Employee&gt; (buffers ALL qualifying employees)
    ///   • One Grouping object per (dept, band) combination
    ///   • SelectMany inside the Select for billable hours (new iterator per group)
    ///   • Anonymous type instance per output row
    ///   • OrderBy internal sort buffer (copies all rows into an array for sort)
    /// </summary>
    [Benchmark(Description = "LINQ: SelectMany → Where → GroupBy → Select → OrderBy")]
    public List<DeptBandReport> Linq_FullPipeline()
    {
        return _companies
            .SelectMany(c => c.Departments
                .SelectMany(d => d.Employees
                    .Where(e => e.IsActive)
                    .Select(e => new { Dept = d.Name, Employee = e })))
            .GroupBy(x => new { x.Dept, Band = GetBand(x.Employee.Salary) })
            .Select(g => new DeptBandReport
            {
                Department = g.Key.Dept,
                Band = g.Key.Band,
                EmployeeCount = g.Count(),
                AverageSalary = g.Average(x => x.Employee.Salary),
                // Inner SelectMany re-enumerates every employee's projects+entries
                TotalBillableHours = g
                    .SelectMany(x => x.Employee.Projects)
                    .SelectMany(p => p.TimeEntries)
                    .Where(t => t.IsBillable)
                    .Sum(t => t.Hours)
            })
            .OrderBy(r => r.Department)
            .ThenBy(r => r.Band)
            .ToList();
    }

    /// <summary>
    /// Same pipeline, but Count() is called separately before the main Select
    /// — a very common mistake when you want the count for logging/validation.
    /// GroupBy buffers all data, then Count() iterates the groups, then Select
    /// iterates them again.
    /// </summary>
    [Benchmark(Description = "LINQ: GroupBy + Count() before Select (triple enumeration)")]
    public List<DeptBandReport> Linq_CountBeforeSelect()
    {
        var groups = _companies
            .SelectMany(c => c.Departments
                .SelectMany(d => d.Employees
                    .Where(e => e.IsActive)
                    .Select(e => new { Dept = d.Name, Employee = e })))
            .GroupBy(x => new { x.Dept, Band = GetBand(x.Employee.Salary) });

        _ = groups.Count();   // first full pass over groups

        return groups          // second full pass
            .Select(g => new DeptBandReport
            {
                Department = g.Key.Dept,
                Band = g.Key.Band,
                EmployeeCount = g.Count(),
                AverageSalary = g.Average(x => x.Employee.Salary),
                TotalBillableHours = g
                    .SelectMany(x => x.Employee.Projects)
                    .SelectMany(p => p.TimeEntries)
                    .Where(t => t.IsBillable)
                    .Sum(t => t.Hours)
            })
            .OrderBy(r => r.Department)
            .ThenBy(r => r.Band)
            .ToList();
    }

    // ─── Foreach equivalent ───────────────────────────────────────────────────

    /// <summary>
    /// Accumulates into a Dictionary in a single pass over the data.
    /// The TryGetValue / Add pattern avoids any intermediate collection.
    /// Sorting only the final list (which is small — one row per group).
    /// </summary>
    [Benchmark(Description = "Foreach: Dictionary accumulation + sort output (baseline)", Baseline = true)]
    public List<DeptBandReport> Foreach_DictionaryAccumulation()
    {
        var acc = new Dictionary<(string Dept, string Band), Accumulator>();

        foreach (var company in _companies)
        foreach (var dept in company.Departments)
        foreach (var employee in dept.Employees)
        {
            if (!employee.IsActive) continue;

            var key = (dept.Name, GetBand(employee.Salary));
            if (!acc.TryGetValue(key, out var entry))
            {
                entry = new Accumulator { Department = dept.Name, Band = key.Item2 };
                acc[key] = entry;
            }

            entry.Count++;
            entry.SalarySum += employee.Salary;

            foreach (var project in employee.Projects)
            foreach (var te in project.TimeEntries)
            {
                if (te.IsBillable)
                    entry.BillableHours += te.Hours;
            }
        }

        var result = new List<DeptBandReport>(acc.Count);
        foreach (var kvp in acc)
        {
            result.Add(new DeptBandReport
            {
                Department = kvp.Value.Department,
                Band = kvp.Value.Band,
                EmployeeCount = kvp.Value.Count,
                AverageSalary = kvp.Value.Count > 0 ? kvp.Value.SalarySum / kvp.Value.Count : 0m,
                TotalBillableHours = kvp.Value.BillableHours
            });
        }

        result.Sort((a, b) =>
        {
            int c = string.Compare(a.Department, b.Department, StringComparison.Ordinal);
            return c != 0 ? c : string.Compare(a.Band, b.Band, StringComparison.Ordinal);
        });

        return result;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static string GetBand(decimal salary) => salary switch
    {
        < 50_000m  => "Junior",
        < 100_000m => "Mid",
        _          => "Senior"
    };

    private sealed class Accumulator
    {
        public string Department = string.Empty;
        public string Band = string.Empty;
        public int Count;
        public decimal SalarySum;
        public double BillableHours;
    }
}

public sealed class DeptBandReport
{
    public string Department { get; set; } = string.Empty;
    public string Band { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal AverageSalary { get; set; }
    public double TotalBillableHours { get; set; }
}
