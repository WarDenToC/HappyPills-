using MedSafety.Core;
using MedSafety.Core.Enum;
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
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAt { get; init; } = DateTime.Now;
    public DateTime? ReviewedAt { get; set; }
}

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

    // FR9/FR10: rejecting needs a reason; approving a Critical prescription needs an override reason.
    public (bool Success, string? Error) Decide(int id, string decision, string? reason)
    {
        var record = _records.FirstOrDefault(r => r.Id == id);
        if (record is null) return (false, "Prescription not found.");
        if (record.Status != "Submitted") return (false, "This prescription has already been reviewed.");

        switch (decision)
        {
            case "Rejected":
                if (string.IsNullOrWhiteSpace(reason))
                    return (false, "A reason is required to reject a prescription.");
                record.RejectionReason = reason.Trim();
                break;

            case "Approved":
                if (record.SafetyResult.Outcome == Severity.Critical)
                {
                    if (string.IsNullOrWhiteSpace(reason))
                        return (false, "A reason is required to override a Critical alert.");
                    record.OverrideReason = reason.Trim();
                }
                break;

            default:
                return (false, $"Unknown decision: {decision}");
        }

        record.Status = decision;
        record.ReviewedAt = DateTime.Now;
        return (true, null);
    }
}