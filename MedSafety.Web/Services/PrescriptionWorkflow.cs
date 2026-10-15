using MedSafety.Core;
using MedSafety.Core.Audit;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Security;

namespace MedSafety.Web.Services;

// Ties the prescribe - review flow to authorisation and the audit log
// Denials are written to the audit log by AuthorizationService, this class writes an entry
// for every submission, every alert raised and every review decision
public class PrescriptionWorkflow
{
    private readonly PrescriptionStore _store;
    private readonly AuthorizationService _authorization;
    private readonly IAuditLog _audit;

    public PrescriptionWorkflow(PrescriptionStore store, AuthorizationService authorization, IAuditLog audit)
    {
        _store = store;
        _authorization = authorization;
        _audit = audit;
    }

    public IReadOnlyList<PrescriptionRecord> All => _store.All;

    public PrescriptionRecord? Submit(string actor, Role role, Prescription rx, PatientContext patient, SafetyCheckResult result)
    {
        if (!_authorization.Authorize(actor, role, Permission.CreatePrescription))
            return null;

        var record = _store.Submit(rx, patient, result);
        Log(actor, role, "PrescriptionSubmitted",
            $"RX-{record.Id}: {rx.DrugName} {rx.DoseAmount}{rx.DoseUnit} for {patient.Firstname} {patient.Lastname}; outcome {result.Outcome}");

        foreach (var alert in result.Alerts)
        {
            Log(actor, role, "AlertRaised",
                $"RX-{record.Id}: {alert.RuleType} ({alert.Severity}) - {alert.Message}");
        }

        return record;
    }

    public (bool Success, string? Error) Review(string actor, Role role, int id, string decision, string? reason)
    {
        if (!_authorization.Authorize(actor, role, Permission.ReviewPrescription))
            return (false, "AccessDenied: only a Pharmacist can review prescriptions.");

        var (success, error) = _store.Decide(id, decision, reason);
        if (!success) return (false, error);

        var record = _store.All.First(r => r.Id == id);
        var action = decision == "Approved" && record.SafetyResult.Outcome == Severity.Critical
            ? "OverrideApproved"
            : decision;
        var detail = $"RX-{id}: {decision}" + (string.IsNullOrWhiteSpace(reason) ? "" : $" - reason: {reason.Trim()}");

        Log(actor, role, action, detail);
        return (true, null);
    }

    private void Log(string actor, Role role, string action, string detail) =>
        _audit.Write(new AuditEntry(DateTime.UtcNow, actor, role, action, detail));
}