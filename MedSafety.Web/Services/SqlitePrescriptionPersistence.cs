using System.Text.Json;
using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Data;
using MedSafety.Data.Rows;
using Microsoft.EntityFrameworkCore;

namespace MedSafety.Web.Services;

public class StoredAlert
{
    public Severity Severity { get; set; }
    public RuleType RuleType { get; set; }
    public string Message { get; set; } = "";
}

// FR12: prescriptions and their review outcomes survive an application restart.
// Lives in Web because PrescriptionRecord is a Web workflow type.
public class SqlitePrescriptionPersistence : IPrescriptionPersistence
{
    private readonly IDbContextFactory<MedSafetyDbContext> _factory;

    public SqlitePrescriptionPersistence(IDbContextFactory<MedSafetyDbContext> factory)
    {
        _factory = factory;
    }

    public IReadOnlyList<PrescriptionRecord> LoadAll()
    {
        using var db = _factory.CreateDbContext();
        var patients = db.Patients.ToList().ToDictionary(p => p.Id);

        return db.Prescriptions.OrderBy(p => p.Id).ToList()
            .Select(row => ToRecord(row, patients))
            .ToList();
    }

    public int Insert(PrescriptionRecord record)
    {
        using var db = _factory.CreateDbContext();
        var row = ToRow(record);
        db.Prescriptions.Add(row);
        db.SaveChanges();
        return row.Id;
    }

    public void Update(PrescriptionRecord record)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Prescriptions.Find(record.Id);
        if (row is null) return;

        row.Status = record.Status;
        row.OverrideReason = record.OverrideReason;
        row.RejectionReason = record.RejectionReason;
        row.ReviewedAt = record.ReviewedAt;
        db.SaveChanges();
    }

    private static PrescriptionRow ToRow(PrescriptionRecord r) => new()
    {
        PatientId = r.Prescription.PatientId,
        DrugName = r.Prescription.DrugName,
        DoseUnit = r.Prescription.DoseUnit,
        DoseAmount = r.Prescription.DoseAmount,
        Frequency = r.Prescription.Frequency,
        Route = r.Prescription.Route,
        Duration = r.Prescription.Duration,
        Outcome = r.SafetyResult.Outcome,
        AlertsJson = JsonSerializer.Serialize(r.SafetyResult.Alerts.Select(a =>
            new StoredAlert { Severity = a.Severity, RuleType = a.RuleType, Message = a.Message }).ToList()),
        Status = r.Status,
        OverrideReason = r.OverrideReason,
        RejectionReason = r.RejectionReason,
        SubmittedAt = r.SubmittedAt,
        ReviewedAt = r.ReviewedAt
    };

    private static PrescriptionRecord ToRecord(PrescriptionRow row, Dictionary<int, PatientRow> patients)
    {
        var alerts = (JsonSerializer.Deserialize<List<StoredAlert>>(row.AlertsJson) ?? new List<StoredAlert>())
            .Select(a => new Alert(a.Severity, a.RuleType, a.Message))
            .ToList();

        var patient = patients.TryGetValue(row.PatientId, out var p)
            ? new PatientContext(p.Id, p.Firstname, p.Lastname, p.DrugAllergies, p.ClassAllergies, p.ActiveMeds)
            : new PatientContext(row.PatientId, "Unknown", "Patient", new(), new(), new());

        return new PrescriptionRecord
        {
            Id = row.Id,
            Prescription = new Prescription(row.PatientId, row.DrugName, row.DoseUnit,
                row.DoseAmount, row.Frequency, row.Route, row.Duration),
            Patient = patient,
            SafetyResult = new SafetyCheckResult(alerts, row.Outcome),
            Status = row.Status,
            OverrideReason = row.OverrideReason,
            RejectionReason = row.RejectionReason,
            SubmittedAt = row.SubmittedAt,
            ReviewedAt = row.ReviewedAt
        };
    }
}