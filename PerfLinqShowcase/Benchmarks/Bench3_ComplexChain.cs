using BenchmarkDotNet.Attributes;
using PerfLinqShowcase.DataGeneration;
using PerfLinqShowcase.Models;

namespace PerfLinqShowcase.Benchmarks;

/// <summary>
/// BENCHMARK 3 — Long operator chain with DTO projection
///
/// Task: Build a report DTO per active employee that includes:
///       name, total billable hours, total billable revenue, active project count,
///       and their highest-earning project.
///
/// Demonstrates:
///   • 5+ chained operators on a flat employee list (Where → SelectMany → Where →
///     Sum, Select, Count, OrderByDescending, First) evaluated repeatedly per employee.
///   • For each employee the LINQ approach re-enumerates Projects multiple times
///     (once for billable hours, once for revenue, once for count, once for max).
///   • The foreach version computes everything in a single pass over Projects
///     with a handful of scalar variables.
///   • Memory: each Select producing an anonymous type + each OrderBy building
///     an intermediate sorted array = O(N × P) allocations.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class Bench3_ComplexChain
{
    private List<Employee> _employees = null!;

    [Params(100, 1_000, 10_000)]
    public int EmployeeCount;

    // Each employee gets this many projects, each with this many time entries.
    private const int ProjectsPerEmployee = 5;
    private const int EntriesPerProject = 15;

    [GlobalSetup]
    public void Setup()
    {
        var data = DataGenerator.Generate(
            companies: 1,
            deptsPerCompany: 1,
            employeesPerDept: EmployeeCount,
            projectsPerEmployee: ProjectsPerEmployee,
            timeEntriesPerProject: EntriesPerProject);

        _employees = data[0].Departments[0].Employees;
    }

    // ─── LINQ versions ────────────────────────────────────────────────────────

    /// <summary>
    /// Four separate LINQ sub-queries per employee. Each call to .Where(),
    /// .SelectMany(), .Sum(), .Count() is a fresh iterator walk over Projects/Entries.
    /// For 10k employees × 5 projects × 15 entries that is a lot of re-traversal.
    /// </summary>
    [Benchmark(Description = "LINQ: 4 sub-queries per employee (re-enumerating Projects)")]
    public List<EmployeeReportDto> Linq_MultipleSubQueries()
    {
        return _employees
            .Where(e => e.IsActive)
            .Select(e => new EmployeeReportDto
            {
                Name = e.Name,
                // Sub-query 1: total billable hours — walks all projects+entries
                TotalBillableHours = e.Projects
                    .SelectMany(p => p.TimeEntries)
                    .Where(t => t.IsBillable)
                    .Sum(t => t.Hours),

                // Sub-query 2: total billable revenue — SAME traversal again
                TotalBillableRevenue = e.Projects
                    .SelectMany(p => p.TimeEntries)
                    .Where(t => t.IsBillable)
                    .Sum(t => (decimal)t.Hours * t.HourlyRate),

                // Sub-query 3: active project count — another iteration
                ActiveProjectCount = e.Projects
                    .Count(p => p.Status == "Active"),

                // Sub-query 4: best project by budget — sorts then takes first
                TopProjectName = e.Projects
                    .OrderByDescending(p => p.Budget)
                    .Select(p => p.Name)
                    .FirstOrDefault() ?? "(none)"
            })
            .ToList();
    }

    /// <summary>
    /// Same as above but also calls Count() before the main Select, causing yet
    /// another full pass through the Where iterator (double-enumeration anti-pattern).
    /// </summary>
    [Benchmark(Description = "LINQ: Count() then Select() — double enumeration of Where")]
    public List<EmployeeReportDto> Linq_DoubleEnumerateOuter()
    {
        var active = _employees.Where(e => e.IsActive);
        _ = active.Count(); // forces a full iteration to get the count

        return active          // iterates AGAIN
            .Select(e => new EmployeeReportDto
            {
                Name = e.Name,
                TotalBillableHours = e.Projects
                    .SelectMany(p => p.TimeEntries)
                    .Where(t => t.IsBillable)
                    .Sum(t => t.Hours),
                TotalBillableRevenue = e.Projects
                    .SelectMany(p => p.TimeEntries)
                    .Where(t => t.IsBillable)
                    .Sum(t => (decimal)t.Hours * t.HourlyRate),
                ActiveProjectCount = e.Projects.Count(p => p.Status == "Active"),
                TopProjectName = e.Projects
                    .OrderByDescending(p => p.Budget)
                    .Select(p => p.Name)
                    .FirstOrDefault() ?? "(none)"
            })
            .ToList();
    }

    // ─── Foreach equivalent ───────────────────────────────────────────────────

    /// <summary>
    /// Single loop over employees; single inner loop over projects+entries.
    /// All four metrics computed in one pass — no re-traversal, no intermediate
    /// allocations beyond the output list and the DTO objects themselves.
    /// </summary>
    [Benchmark(Description = "Foreach: single pass per employee (baseline)", Baseline = true)]
    public List<EmployeeReportDto> Foreach_SinglePass()
    {
        var result = new List<EmployeeReportDto>(_employees.Count);

        foreach (var e in _employees)
        {
            if (!e.IsActive) continue;

            double totalHours = 0;
            decimal totalRevenue = 0m;
            int activeProjects = 0;
            string topProject = "(none)";
            decimal topBudget = -1m;

            foreach (var p in e.Projects)
            {
                if (p.Status == "Active") activeProjects++;

                if (p.Budget > topBudget)
                {
                    topBudget = p.Budget;
                    topProject = p.Name;
                }

                foreach (var t in p.TimeEntries)
                {
                    if (!t.IsBillable) continue;
                    totalHours += t.Hours;
                    totalRevenue += (decimal)t.Hours * t.HourlyRate;
                }
            }

            result.Add(new EmployeeReportDto
            {
                Name = e.Name,
                TotalBillableHours = totalHours,
                TotalBillableRevenue = totalRevenue,
                ActiveProjectCount = activeProjects,
                TopProjectName = topProject
            });
        }

        return result;
    }
}

public sealed class EmployeeReportDto
{
    public string Name { get; set; } = string.Empty;
    public double TotalBillableHours { get; set; }
    public decimal TotalBillableRevenue { get; set; }
    public int ActiveProjectCount { get; set; }
    public string TopProjectName { get; set; } = string.Empty;
}
