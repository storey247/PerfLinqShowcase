using BenchmarkDotNet.Attributes;
using PerfLinqShowcase.DataGeneration;
using PerfLinqShowcase.Models;

namespace PerfLinqShowcase.Benchmarks;

/// <summary>
/// BENCHMARK 1 — Simple filter + transform + aggregation
///
/// Task: Sum salaries of all active employees earning over £50k.
///
/// Demonstrates:
///   • Each chained LINQ operator (Where → Select → Sum) traverses the collection
///     independently, each wrapping the previous in a new iterator object.
///   • The foreach version does it all in a single pass with zero heap allocation.
///   • At large scale the iterator overhead and deferred-execution wrapper objects
///     become measurable GC pressure even for trivial operations.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class Bench1_SimpleFilterTransform
{
    private List<Employee> _employees = null!;

    [Params(500, 5_000, 50_000)]
    public int EmployeeCount;

    [GlobalSetup]
    public void Setup()
    {
        // Use a flat employee list — this benchmark isolates the basic chain cost.
        var rng = new Random(42);
        _employees = Enumerable.Range(1, EmployeeCount)
            .Select(i => new Employee
            {
                Id = i,
                Name = $"Employee_{i:D6}",
                IsActive = rng.NextDouble() > 0.15,
                Salary = rng.Next(20_000, 250_000),
                Role = "Engineer",
                HireDate = DateTime.Today.AddDays(-rng.Next(30, 4000)),
                YearsExperience = rng.Next(0, 20),
                Location = "London",
                Skills = [],
                Projects = []
            })
            .ToList();
    }

    // ─── LINQ versions ────────────────────────────────────────────────────────

    /// <summary>
    /// 3-operator LINQ chain. Creates two intermediate iterator objects
    /// (WhereIterator, SelectIterator) before Sum pulls values through.
    /// </summary>
    [Benchmark(Description = "LINQ: Where → Select → Sum (3 operators)")]
    public decimal Linq_WhereSelectSum()
    {
        return _employees
            .Where(e => e.IsActive && e.Salary > 50_000m)
            .Select(e => e.Salary)
            .Sum();
    }

    /// <summary>
    /// Naively calling Count() before Sum() — a common mistake that forces
    /// TWO full iterations through the Where iterator.
    /// </summary>
    [Benchmark(Description = "LINQ: Where → Count + Where → Sum (double enumeration)")]
    public (int count, decimal total) Linq_DoubleEnumeration()
    {
        var query = _employees.Where(e => e.IsActive && e.Salary > 50_000m);
        return (query.Count(), query.Sum(e => e.Salary)); // iterates TWICE
    }

    /// <summary>
    /// Extra ToList() materialisation that many developers add "for safety",
    /// which copies the filtered set onto the heap before summing.
    /// </summary>
    [Benchmark(Description = "LINQ: Where → ToList → Select → Sum (unnecessary materialisation)")]
    public decimal Linq_UnnecessaryToList()
    {
        return _employees
            .Where(e => e.IsActive && e.Salary > 50_000m)
            .ToList()             // ← copies filtered results into a new List<T>
            .Select(e => e.Salary)
            .Sum();
    }

    // ─── Foreach equivalent ───────────────────────────────────────────────────

    /// <summary>
    /// Single pass, zero allocation. One variable, one loop, one predicate.
    /// The compiler/JIT can also hoist bounds checks and inline the body.
    /// </summary>
    [Benchmark(Description = "Foreach: single-pass sum (baseline)", Baseline = true)]
    public (int count, decimal total) Foreach_SinglePass()
    {
        int count = 0;
        decimal total = 0m;
        foreach (var e in _employees)
        {
            if (e.IsActive && e.Salary > 50_000m)
            {
                count++;
                total += e.Salary;
            }
        }
        return (count, total);
    }
}
