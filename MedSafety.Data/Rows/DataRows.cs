using MedSafety.Core.Enum;
using MedSafety.Core.Security;

namespace MedSafety.Data.Rows;

// Table shapes for SQLite. Kept separate from the Core models for easier usage.
public class DrugRow
{
    public int Id { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public DrugClass DrugClass { get; set; }
    public DoseUnit DoseUnit { get; set; }
    public decimal MinDose { get; set; }
    public decimal MaxDose { get; set; }
}

public class InteractionRow
{
    public int Id { get; set; }
    public string DrugA { get; set; } = string.Empty;
    public string DrugB { get; set; } = string.Empty;
    public Severity Severity { get; set; }
}

public class PatientRow
{
    public int Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public List<string> DrugAllergies { get; set; } = new();
    public List<string> ClassAllergies { get; set; } = new();
    public List<string> ActiveMeds { get; set; } = new();
}

public class AuditRow
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Actor { get; set; } = string.Empty;
    public Role Role { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}