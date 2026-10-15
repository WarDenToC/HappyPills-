using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;

namespace MedSafety.Web.Services;

public class PrescriptionRecord
{
    public int Id { get; set; }
    public required Prescription Prescription { get; init; }
    public required PatientContext Patient { get; init; }
    public required SafetyCheckResult SafetyResult { get; init; }
    public string Status { get; set; } = "Submitted";
    public string? OverrideReason { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAt { get; init; } = DateTime.Now;
    public DateTime? ReviewedAt { get; set; }
}

// Holds prescriptions in memory and writes through to persistence when one is supplied.
// With no persistence (as in most unit tests) it behaves as a plain in-memory store.
public class PrescriptionStore
{
    private readonly object _lock = new();
    private readonly List<PrescriptionRecord> _records;
    private readonly IPrescriptionPersistence? _persistence;
    private int _nextId = 1;

    public PrescriptionStore(IPrescriptionPersistence? persistence = null)
    {
        _persistence = persistence;
        _records = persistence is null
            ? new List<PrescriptionRecord>()
            : persistence.LoadAll().ToList();
    }

    // A snapshot, so pages can enumerate it while another session submits
    public IReadOnlyList<PrescriptionRecord> All
    {
        get
        {
            lock (_lock)
            {
                return _records.ToList();
            }
        }
    }

    public PrescriptionRecord Submit(Prescription prescription, PatientContext patient, SafetyCheckResult result)
    {
        lock (_lock)
        {
            var record = new PrescriptionRecord
            {
                Prescription = prescription,
                Patient = patient,
                SafetyResult = result
            };
            record.Id = _persistence?.Insert(record) ?? _nextId++;
            _records.Add(record);
            return record;
        }
    }

    // FR9/FR10: rejecting needs a reason; approving a Critical prescription needs an override reason.
    public (bool Success, string? Error) Decide(int id, string decision, string? reason)
    {
        lock (_lock)
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
            _persistence?.Update(record);
            return (true, null);
        }
    }
}