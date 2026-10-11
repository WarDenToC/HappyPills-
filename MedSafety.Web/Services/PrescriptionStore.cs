using MedSafety.Core;
using MedSafety.Core.Models;

namespace MedSafety.Web.Services;

public class PrescriptionRecord
{
    public int Id { get; init; }
    public required Prescription Prescription { get; init; }
    public required PatientContext Patient { get; init; }
    public required SafetyCheckResult SafetyResult { get; init; }
    public string Status { get; set; } = "Submitted";
    public string? OverrideReason { get; set; }
    public DateTime SubmittedAt { get; init; } = DateTime.Now;
}

// In-memory stand-in for MedSafety.Data persistence. same public shape will
// move behind an interface once EF Core/SQLite is ready.
public class PrescriptionStore
{
    private readonly List<PrescriptionRecord> _records = new();
    private int _nextId = 1;

    public IReadOnlyList<PrescriptionRecord> All => _records;

    public PrescriptionRecord Submit(Prescription prescription, PatientContext patient, SafetyCheckResult result)
    {
        var record = new PrescriptionRecord
        {
            Id = _nextId++,
            Prescription = prescription,
            Patient = patient,
            SafetyResult = result
        };
        _records.Add(record);
        return record;
    }

    // FR9/FR10: Critical outcome cannot be approved without a recorded override reason.
    public (bool Success, string? Error) Decide(int id, string decision, string? overrideReason)
    {
        var record = _records.FirstOrDefault(r => r.Id == id);
        if (record is null) return (false, "Prescription not found.");

        if (decision == "Approved" && record.SafetyResult.Outcome == Core.Enum.Severity.Critical)
        {
            if (string.IsNullOrWhiteSpace(overrideReason))
            {
                return (false, "A reason is required to override a Critical alert.");
            }
            record.OverrideReason = overrideReason;
            record.Status = "Approved";
            return (true, null);
        }

        record.Status = decision;
        return (true, null);
    }
}