using BenchmarkDotNet.Attributes;
using PerfLinqShowcase.DataGeneration;
using PerfLinqShowcase.Models;

namespace PerfLinqShowcase.Benchmarks;

/// <summary>
/// BENCHMARK 2 — Flattening deep nested collections with SelectMany
///
/// Task: Collect all time entries across Company → Department → Employee → Project.
///
/// Demonstrates:
///   • Each SelectMany wraps the previous enumerator. The innermost TimeEntry
///     data is reached through a 4-level iterator stack on every MoveNext().
///   • ToList() at the end materialises the entire flat sequence onto the heap.
///   • The foreach version navigates the same structure but with direct field
///     access and no per-level heap allocation.
///   • As depth × breadth grows, the iterator overhead compounds significantly.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class Bench2_NestedSelectMany
{
    private List<Company> _companies = null!;

    // Scale: companies × depts × employees × projects × timeEntries
    //   Small  →  2×3×10×2×5   =   600 time entries
    //   Medium →  5×4×20×3×10  = 12,000 time entries
    //   Large  → 10×5×40×4×20  = 160,000 time entries
    [Params("Small", "Medium", "Large")]
    public string Scale = "Small";

    [GlobalSetup]
    public void Setup()
    {
        _companies = Scale switch
        {
            "Small"  => DataGenerator.Generate(2,  3, 10,  2,  5),
            "Medium" => DataGenerator.Generate(5,  4, 20,  3, 10),
            "Large"  => DataGenerator.Generate(10, 5, 40,  4, 20),
            _        => throw new ArgumentOutOfRangeException(nameof(Scale))
        };
    }

    // ─── LINQ versions ────────────────────────────────────────────────────────

    /// <summary>
    /// Classic 4-level SelectMany chain. Each level creates a SelectManyIterator
    /// wrapping the previous. Every call to MoveNext() on the outermost iterator
    /// bounces through 4 virtual dispatch calls before yielding a value.
    /// </summary>
    [Benchmark(Description = "LINQ: 4× SelectMany → ToList")]
    public List<TimeEntry> Linq_DeepSelectMany()
    {
        return _companies
            .SelectMany(c => c.Departments)
            .SelectMany(d => d.Employees)
            .SelectMany(e => e.Projects)
            .SelectMany(p => p.TimeEntries)
            .ToList();
    }

    /// <summary>
    /// Filtered flattening — adds a Where at each level. Now each level
    /// adds BOTH a SelectManyIterator AND a WhereIterator to the stack.
    /// </summary>
    [Benchmark(Description = "LINQ: 4× SelectMany + Where filters → ToList")]
    public List<TimeEntry> Linq_DeepSelectManyWithFilters()
    {
        return _companies
            .Where(c => c.Departments.Count > 0)
            .SelectMany(c => c.Departments)
            .Where(d => d.Employees.Count > 0)
            .SelectMany(d => d.Employees)
            .Where(e => e.IsActive)
            .SelectMany(e => e.Projects)
            .Where(p => p.Status == "Active")
            .SelectMany(p => p.TimeEntries)
            .Where(t => t.IsBillable)
            .ToList();
    }

    /// <summary>
    /// Projection during flattening — a common pattern that creates anonymous
    /// type instances (heap allocation) for every time entry.
    /// </summary>
    [Benchmark(Description = "LINQ: SelectMany with projection → anonymous type list")]
    public List<object> Linq_SelectManyWithProjection()
    {
        return _companies
            .SelectMany(c => c.Departments
                .SelectMany(d => d.Employees
                    .Where(e => e.IsActive)
                    .SelectMany(e => e.Projects
                        .SelectMany(p => p.TimeEntries
                            .Select(t => new
                            {
                                CompanyName = c.Name,
                                DeptName = d.Name,
                                EmployeeName = e.Name,
                                ProjectName = p.Name,
                                t.Hours,
                                t.IsBillable
                            })))))
            .Cast<object>()
            .ToList();
    }

    // ─── Foreach equivalent ───────────────────────────────────────────────────

    /// <summary>
    /// Direct nested loops. All four nesting levels use the List<T> indexer path
    /// so the JIT can eliminate bounds checks. No heap allocation except the
    /// output list itself.
    /// </summary>
    [Benchmark(Description = "Foreach: nested loops → List (baseline)", Baseline = true)]
    public List<TimeEntry> Foreach_NestedLoops()
    {
        var result = new List<TimeEntry>();
        foreach (var company in _companies)
        foreach (var dept in company.Departments)
        foreach (var employee in dept.Employees)
        foreach (var project in employee.Projects)
        foreach (var entry in project.TimeEntries)
            result.Add(entry);
        return result;
    }

    /// <summary>
    /// Filtered nested loops — mirrors Linq_DeepSelectManyWithFilters.
    /// </summary>
    [Benchmark(Description = "Foreach: nested loops with filters (baseline filtered)")]
    public List<TimeEntry> Foreach_NestedLoopsFiltered()
    {
        var result = new List<TimeEntry>();
        foreach (var company in _companies)
        {
            if (company.Departments.Count == 0) continue;
            foreach (var dept in company.Departments)
            {
                if (dept.Employees.Count == 0) continue;
                foreach (var employee in dept.Employees)
                {
                    if (!employee.IsActive) continue;
                    foreach (var project in employee.Projects)
                    {
                        if (project.Status != "Active") continue;
                        foreach (var entry in project.TimeEntries)
                        {
                            if (entry.IsBillable)
                                result.Add(entry);
                        }
                    }
                }
            }
        }
        return result;
    }
}
