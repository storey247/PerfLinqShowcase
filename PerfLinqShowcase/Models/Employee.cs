namespace PerfLinqShowcase.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
    public DateTime HireDate { get; set; }
    public int YearsExperience { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
}
