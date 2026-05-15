namespace PerfLinqShowcase.Models;

public class TimeEntry
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public double Hours { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsBillable { get; set; }
    public decimal HourlyRate { get; set; }
}
