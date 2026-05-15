namespace PerfLinqShowcase.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Active", "Completed", "OnHold"
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<TimeEntry> TimeEntries { get; set; } = [];
}
