namespace MedSafety.Core.Models;

// Synthetic formulary data. Stub shape — partner will populate with seed data (Alphacillin, Betanol, etc.)
public class Formulary
{
    public Dictionary<string, (double Min, double Max)> DoseRanges { get; set; } = new();
    public Dictionary<string, string> DrugClasses { get; set; } = new();
    public List<(string DrugA, string DrugB, Severity Severity)> Interactions { get; set; } = new();
}