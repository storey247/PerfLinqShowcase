using BenchmarkDotNet.Attributes;
using PerfLinqShowcase.DataGeneration;
using PerfLinqShowcase.Models;

namespace PerfLinqShowcase.Benchmarks;

/// <summary>
/// BENCHMARK 4 — Nested Any / All predicate chains
///
/// Task: Find employees who:
///       (a) are active,
///       (b) have at least one active project,
///       (c) on which ALL time entries are billable,
///       (d) and who have the "C#" skill.
///
/// Demonstrates:
///   • Chaining Any() and All() inside a Where() creates deeply nested lambda
///     captures and per-call iterator allocations.
///   • Each Any()/All() on a sub-collection instantiates a fresh enumerator —
///     these are short-lived but numerous, putting pressure on Gen0 GC.
///   • The LINQ version must re-enter the Projects list for each employee via
///     the lambda closure, preventing many JIT optimisations.
///   • Foreach version uses early-exit flags and direct iteration; the JIT can
///     devirtualise, inline predicates, and eliminate bounds checks.
///   • At scale the allocation difference dominates over raw CPU time.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class Bench4_AnyAllPredicates
{
    private List<Company> _companies = null!;

    // Scale: companies × depts × employees × projects × timeEntries
    //   Small  →  3×3×15×3×8   = 3,240 time entries
    //   Medium →  5×5×30×4×15  = 45,000 time entries
    //   Large  → 10×8×50×5×20  = 400,000 time entries
    [Params("Small", "Medium", "Large")]
    public string Scale = "Small";

    [GlobalSetup]
    public void Setup()
    {
        _companies = Scale switch
        {
            "Small"  => DataGenerator.Generate(3,  3, 15,  3,  8),
            "Medium" => DataGenerator.Generate(5,  5, 30,  4, 15),
            "Large"  => DataGenerator.Generate(10, 8, 50,  5, 20),
            _        => throw new ArgumentOutOfRangeException(nameof(Scale))
        };
    }

    // ─── LINQ versions ────────────────────────────────────────────────────────

    /// <summary>
    /// Nested Any(…All(…)) inside a Where. LINQ must:
    ///   1. For each employee: call Where with two lambda predicates
    ///   2. Inside the second predicate: call Any() which spins up a fresh
    ///      enumerator over Projects
    ///   3. Inside the Any predicate: call All() which spins up another
    ///      enumerator over TimeEntries
    ///   Each Any/All allocates a new iterator object on every employee evaluation.
    /// </summary>
    [Benchmark(Description = "LINQ: Where(Any(All(…))) — nested predicate chain")]
    public List<Employee> Linq_NestedAnyAll()
    {
        return _companies
            .SelectMany(c => c.Departments)
            .SelectMany(d => d.Employees)
            .Where(e => e.IsActive
                     && e.Skills.Contains("C#")
                     && e.Projects.Any(p =>
                            p.Status == "Active"
                            && p.TimeEntries.All(t => t.IsBillable)))
            .ToList();
    }

    /// <summary>
    /// Same predicate but materialised to a list mid-chain — a pattern seen when
    /// developers try to "optimise" by breaking the chain. The ToList() snapshot
    /// adds allocation without removing the nested Any/All overhead.
    /// </summary>
    [Benchmark(Description = "LINQ: Where → ToList → Where(Any(All(…))) — spurious materialisation")]
    public List<Employee> Linq_SpuriousMaterialisation()
    {
        var allEmployees = _companies
            .SelectMany(c => c.Departments)
            .SelectMany(d => d.Employees)
            .ToList(); // materialises entire employee population

        return allEmployees
            .Where(e => e.IsActive
                     && e.Skills.Contains("C#")
                     && e.Projects.Any(p =>
                            p.Status == "Active"
                            && p.TimeEntries.All(t => t.IsBillable)))
            .ToList();
    }

    /// <summary>
    /// Repeated Contains() on List vs a HashSet — a subtle allocation trap:
    /// Skills is a List&lt;string&gt; so Contains is O(N). Wrapping it in a HashSet
    /// per employee is a common "fix" that adds a new allocation per employee.
    /// </summary>
    [Benchmark(Description = "LINQ: new HashSet per employee inside Where — micro-allocation trap")]
    public List<Employee> Linq_HashSetPerEmployee()
    {
        return _companies
            .SelectMany(c => c.Departments)
            .SelectMany(d => d.Employees)
            .Where(e =>
            {
                var skills = new HashSet<string>(e.Skills); // allocated per employee!
                return e.IsActive
                    && skills.Contains("C#")
                    && e.Projects.Any(p =>
                            p.Status == "Active"
                            && p.TimeEntries.All(t => t.IsBillable));
            })
            .ToList();
    }

    // ─── Foreach equivalent ───────────────────────────────────────────────────

    /// <summary>
    /// Manual nested loops with early-exit flags. No enumerator objects created
    /// beyond those implied by foreach on List&lt;T&gt; (which the JIT specialises to
    /// a struct enumerator — no heap allocation). All conditions short-circuit
    /// exactly as Any/All would, but without the per-call allocation overhead.
    /// </summary>
    [Benchmark(Description = "Foreach: nested loops with early exit (baseline)", Baseline = true)]
    public List<Employee> Foreach_NestedLoops()
    {
        var result = new List<Employee>();

        foreach (var company in _companies)
        foreach (var dept in company.Departments)
        foreach (var employee in dept.Employees)
        {
            if (!employee.IsActive) continue;

            bool hasSkill = false;
            foreach (var skill in employee.Skills)
            {
                if (skill == "C#") { hasSkill = true; break; }
            }
            if (!hasSkill) continue;

            bool qualifies = false;
            foreach (var project in employee.Projects)
            {
                if (project.Status != "Active") continue;

                bool allBillable = true;
                foreach (var entry in project.TimeEntries)
                {
                    if (!entry.IsBillable) { allBillable = false; break; }
                }

                if (allBillable) { qualifies = true; break; }
            }

            if (qualifies) result.Add(employee);
        }

        return result;
    }
}
