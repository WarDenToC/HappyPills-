using System.Diagnostics;
using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Web.Services;

namespace MedSafety.Tests;

[TestClass]
public class DashboardMetricsTests
{
    private static PrescriptionRecord Make(int id, string status, List<Alert>? alerts = null,
        string? overrideReason = null, string? rejectionReason = null,
        DateTime? submitted = null, DateTime? reviewed = null)
    {
        var list = alerts ?? new List<Alert>();
        var outcome = list.Count == 0 ? Severity.None : list.Max(a => a.Severity);
        return new PrescriptionRecord
        {
            Id = id,
            Prescription = new Prescription(id, "Betanol", DoseUnit.Mg, 50, 1, "Oral", "7 days"),
            Patient = new PatientContext(id, "Test", "Patient", new(), new(), new()),
            SafetyResult = new SafetyCheckResult(list, outcome),
            Status = status,
            OverrideReason = overrideReason,
            RejectionReason = rejectionReason,
            SubmittedAt = submitted ?? new DateTime(2026, 10, 10, 9, 0, 0),
            ReviewedAt = reviewed
        };
    }

    [TestMethod]
    public void Calculate_NoRecords_ReturnsZerosAndAllRuleTypes()
    {
        var summary = DashboardMetrics.Calculate(new List<PrescriptionRecord>());

        Assert.AreEqual(0, summary.Total);
        Assert.AreEqual(0.0, summary.OverrideRatePercent);
        Assert.IsNull(summary.AverageReviewMinutes);
        Assert.AreEqual(4, summary.AlertsByType.Count);
    }

    [TestMethod]
    public void Calculate_CountsEachStatusSeparately()
    {
        var records = new List<PrescriptionRecord>
        {
            Make(1, "Submitted"),
            Make(2, "Approved"),
            Make(3, "Approved", overrideReason: "Specialist consult"),
            Make(4, "Rejected", rejectionReason: "Dose too high")
        };

        var summary = DashboardMetrics.Calculate(records);

        Assert.AreEqual(4, summary.Total);
        Assert.AreEqual(1, summary.Pending);
        Assert.AreEqual(1, summary.Approved);
        Assert.AreEqual(1, summary.Overridden);
        Assert.AreEqual(1, summary.Rejected);
    }

    [TestMethod]
    public void Calculate_AlertsByTypeAndSeverity_CountsEachCell()
    {
        var records = new List<PrescriptionRecord>
        {
            Make(1, "Submitted", new List<Alert>
            {
                new(Severity.Critical, RuleType.DoseRange, "over"),
                new(Severity.Critical, RuleType.Allergies, "allergy")
            }),
            Make(2, "Submitted", new List<Alert>
            {
                new(Severity.Critical, RuleType.Allergies, "allergy"),
                new(Severity.Low, RuleType.Duplicate, "same class")
            })
        };

        var summary = DashboardMetrics.Calculate(records);

        Assert.AreEqual(4, summary.TotalAlerts);
        Assert.AreEqual(2, summary.AlertsByType.First(r => r.RuleType == RuleType.Allergies).Critical);
        Assert.AreEqual(1, summary.AlertsByType.First(r => r.RuleType == RuleType.DoseRange).Critical);
        Assert.AreEqual(1, summary.AlertsByType.First(r => r.RuleType == RuleType.Duplicate).Low);
        Assert.AreEqual(0, summary.AlertsByType.First(r => r.RuleType == RuleType.Interaction).Total);
    }

    [TestMethod]
    public void Calculate_RejectionReasons_AreGroupedIgnoringCaseAndWhitespace()
    {
        var records = new List<PrescriptionRecord>
        {
            Make(1, "Rejected", rejectionReason: "Dose too high"),
            Make(2, "Rejected", rejectionReason: "dose too high "),
            Make(3, "Rejected", rejectionReason: "Wrong drug")
        };

        var summary = DashboardMetrics.Calculate(records);

        Assert.AreEqual(2, summary.RejectionReasons.Count);
        Assert.AreEqual(2, summary.RejectionReasons[0].Count);
        Assert.AreEqual(1, summary.RejectionReasons[1].Count);
    }

    [TestMethod]
    public void Calculate_OverrideRate_IsOverriddenOverReviewed()
    {
        var records = new List<PrescriptionRecord>
        {
            Make(1, "Approved"),
            Make(2, "Approved", overrideReason: "Specialist consult"),
            Make(3, "Rejected", rejectionReason: "a"),
            Make(4, "Rejected", rejectionReason: "b"),
            Make(5, "Submitted")   // not reviewed yet, so not counted
        };

        var summary = DashboardMetrics.Calculate(records);

        Assert.AreEqual(25.0, summary.OverrideRatePercent, 0.001);
    }

    [TestMethod]
    public void Calculate_AverageReviewMinutes_AveragesSubmittedToReviewed()
    {
        var start = new DateTime(2026, 10, 10, 9, 0, 0);
        var records = new List<PrescriptionRecord>
        {
            Make(1, "Approved", submitted: start, reviewed: start.AddMinutes(30)),
            Make(2, "Rejected", rejectionReason: "x", submitted: start, reviewed: start.AddMinutes(10))
        };

        var summary = DashboardMetrics.Calculate(records);

        Assert.AreEqual(20.0, summary.AverageReviewMinutes!.Value, 0.001);
    }

    [TestMethod]
    public void Calculate_500Prescriptions_CompletesUnder3Seconds()
    {
        // NFR2: dashboard metrics for a large data set within 3 seconds
        var records = Enumerable.Range(1, 500).Select(i => Make(i,
            i % 3 == 0 ? "Rejected" : "Submitted",
            new List<Alert> { new(Severity.Critical, RuleType.DoseRange, "over max") },
            rejectionReason: i % 3 == 0 ? "Dose too high" : null)).ToList();

        var timer = Stopwatch.StartNew();
        var summary = DashboardMetrics.Calculate(records);
        timer.Stop();

        Assert.AreEqual(500, summary.Total);
        Assert.IsTrue(timer.ElapsedMilliseconds < 3000, $"Took {timer.ElapsedMilliseconds} ms");
    }
}