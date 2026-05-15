using PerfLinqShowcase.Models;

namespace PerfLinqShowcase.DataGeneration;

/// <summary>
/// Generates realistic-looking test data at configurable scale.
/// Keep seed fixed so all benchmarks work on identical data.
/// </summary>
public static class DataGenerator
{
    private static readonly string[] Roles =
        ["Engineer", "Senior Engineer", "Lead Engineer", "Architect", "Manager", "Director", "Analyst", "QA"];

    private static readonly string[] Industries =
        ["Finance", "Healthcare", "Retail", "Technology", "Manufacturing", "Logistics", "Media", "Consulting"];

    private static readonly string[] DeptNames =
        ["Engineering", "Product", "QA", "DevOps", "Data", "Security", "Infrastructure", "UX"];

    private static readonly string[] ProjectStatuses = ["Active", "Completed", "OnHold"];

    private static readonly string[] SkillNames =
        ["C#", "Java", "Python", "SQL", "Kubernetes", "Azure", "AWS", "React", "Angular", "Terraform",
         "Docker", "Go", "Rust", "TypeScript", "PostgreSQL", "Redis", "Kafka", "gRPC", "GraphQL", "LINQ"];

    private static readonly string[] Locations =
        ["London", "Manchester", "Edinburgh", "Dublin", "Amsterdam", "Berlin", "Paris", "New York"];

    /// <summary>
    /// Build a dataset with the given shape.
    /// </summary>
    /// <param name="companies">Number of company objects.</param>
    /// <param name="deptsPerCompany">Departments per company.</param>
    /// <param name="employeesPerDept">Employees per department.</param>
    /// <param name="projectsPerEmployee">Projects per employee.</param>
    /// <param name="timeEntriesPerProject">Time entries per project.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    public static List<Company> Generate(
        int companies,
        int deptsPerCompany,
        int employeesPerDept,
        int projectsPerEmployee,
        int timeEntriesPerProject,
        int seed = 42)
    {
        var rng = new Random(seed);
        var result = new List<Company>(companies);
        int idCounter = 0;

        for (int c = 0; c < companies; c++)
        {
            var company = new Company
            {
                Id = ++idCounter,
                Name = $"Company_{c:D4}",
                Industry = Industries[rng.Next(Industries.Length)],
                Departments = new List<Department>(deptsPerCompany)
            };

            for (int d = 0; d < deptsPerCompany; d++)
            {
                var dept = new Department
                {
                    Id = ++idCounter,
                    Name = DeptNames[d % DeptNames.Length],
                    Location = Locations[rng.Next(Locations.Length)],
                    Budget = rng.Next(50_000, 2_000_000),
                    Employees = new List<Employee>(employeesPerDept)
                };

                for (int e = 0; e < employeesPerDept; e++)
                {
                    int skillCount = rng.Next(3, 8);
                    var skills = new List<string>(skillCount);
                    for (int s = 0; s < skillCount; s++)
                        skills.Add(SkillNames[rng.Next(SkillNames.Length)]);

                    var employee = new Employee
                    {
                        Id = ++idCounter,
                        Name = $"Employee_{idCounter:D6}",
                        Role = Roles[rng.Next(Roles.Length)],
                        Salary = rng.Next(30_000, 200_000),
                        IsActive = rng.NextDouble() > 0.15, // ~85% active
                        HireDate = DateTime.Today.AddDays(-rng.Next(30, 4000)),
                        YearsExperience = rng.Next(0, 20),
                        Location = Locations[rng.Next(Locations.Length)],
                        Skills = skills,
                        Projects = new List<Project>(projectsPerEmployee)
                    };

                    for (int p = 0; p < projectsPerEmployee; p++)
                    {
                        var startDate = DateTime.Today.AddDays(-rng.Next(30, 730));
                        var project = new Project
                        {
                            Id = ++idCounter,
                            Name = $"Project_{idCounter:D6}",
                            Status = ProjectStatuses[rng.Next(ProjectStatuses.Length)],
                            Budget = rng.Next(10_000, 500_000),
                            StartDate = startDate,
                            EndDate = rng.NextDouble() > 0.4 ? startDate.AddDays(rng.Next(30, 365)) : null,
                            TimeEntries = new List<TimeEntry>(timeEntriesPerProject)
                        };

                        for (int t = 0; t < timeEntriesPerProject; t++)
                        {
                            project.TimeEntries.Add(new TimeEntry
                            {
                                Id = ++idCounter,
                                Date = startDate.AddDays(rng.Next(0, 180)),
                                Hours = Math.Round(rng.NextDouble() * 8 + 0.5, 1),
                                Description = $"Work item {t}",
                                IsBillable = rng.NextDouble() > 0.3,
                                HourlyRate = rng.Next(50, 300)
                            });
                        }

                        employee.Projects.Add(project);
                    }

                    dept.Employees.Add(employee);
                }

                company.Departments.Add(dept);
            }

            result.Add(company);
        }

        return result;
    }

    public static string Describe(List<Company> data)
    {
        int depts = data.Sum(c => c.Departments.Count);
        int employees = data.SelectMany(c => c.Departments).Sum(d => d.Employees.Count);
        int projects = data.SelectMany(c => c.Departments).SelectMany(d => d.Employees).Sum(e => e.Projects.Count);
        int entries = data.SelectMany(c => c.Departments)
                          .SelectMany(d => d.Employees)
                          .SelectMany(e => e.Projects)
                          .Sum(p => p.TimeEntries.Count);

        return $"{data.Count} companies | {depts} depts | {employees} employees | {projects} projects | {entries} time entries";
    }
}
