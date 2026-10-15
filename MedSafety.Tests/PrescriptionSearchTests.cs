using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Web.Services;

namespace MedSafety.Tests;

[TestClass]
public class PrescriptionSearchTests
{
    private static PrescriptionRecord Make(int id, string first, string last, Severity outcome,
        string status = "Submitted", DateTime? submitted = null, string? overrideReason = null)
    {
        return new PrescriptionRecord
        {
            Id = id,
            Prescription = new Prescription(id, "Betanol", DoseUnit.Mg, 50, 1, "Oral", "7 days"),
            Patient = new PatientContext(id, first, last, new(), new(), new()),
            SafetyResult = new SafetyCheckResult(new List<Alert>(), outcome),
            Status = status,
            OverrideReason = overrideReason,
            SubmittedAt = submitted ?? new DateTime(2026, 10, 10, 9, 0, 0)
        };
    }

    private static List<PrescriptionRecord> Sample() => new()
    {
        Make(1, "Kai", "Ngata", Severity.Critical, "Approved", new DateTime(2026, 10, 10, 9, 0, 0), "Specialist consult"),
        Make(2, "Riya", "Sharma", Severity.Medium, "Rejected", new DateTime(2026, 10, 11, 9, 0, 0)),
        Make(3, "Noah", "Fletcher", Severity.None, "Submitted", new DateTime(2026, 10, 12, 9, 0, 0)),
        Make(4, "Kai", "Ngata", Severity.Low, "Approved", new DateTime(2026, 10, 12, 15, 0, 0))
    };

    private static List<int> Ids(List<PrescriptionRecord> records) => records.Select(r => r.Id).ToList();

    [TestMethod]
    public void Apply_NoFilter_ReturnsAllNewestFirst()
    {
        var result = PrescriptionSearch.Apply(Sample(), new PrescriptionFilter());
        CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, Ids(result));
    }

    [TestMethod]
    public void Apply_PatientText_IsCaseInsensitiveAndPartial()
    {
        var result = PrescriptionSearch.Apply(Sample(), new PrescriptionFilter(Patient: "kAi"));
        CollectionAssert.AreEqual(new[] { 4, 1 }, Ids(result));
    }

    [TestMethod]
    public void Apply_StatusApproved_ExcludesOverriddenPrescriptions()
    {
        var result = PrescriptionSearch.Apply(Sample(), new PrescriptionFilter(Status: "Approved"));
        CollectionAssert.AreEqual(new[] { 4 }, Ids(result));
    }

    [TestMethod]
    public void Apply_StatusOverridden_ReturnsOnlyApprovedWithOverrideReason()
    {
        var result = PrescriptionSearch.Apply(Sample(), new PrescriptionFilter(Status: "Overridden"));
        CollectionAssert.AreEqual(new[] { 1 }, Ids(result));
    }

    [TestMethod]
    public void Apply_SeverityFilter_ReturnsMatchingOutcomeOnly()
    {
        var result = PrescriptionSearch.Apply(Sample(), new PrescriptionFilter(Outcome: Severity.Critical));
        CollectionAssert.AreEqual(new[] { 1 }, Ids(result));
    }

    [TestMethod]
    public void Apply_DateRange_IsInclusiveOfBothEndDates()
    {
        var filter = new PrescriptionFilter(From: new DateTime(2026, 10, 11), To: new DateTime(2026, 10, 12));
        var result = PrescriptionSearch.Apply(Sample(), filter);
        CollectionAssert.AreEqual(new[] { 4, 3, 2 }, Ids(result));
    }

    [TestMethod]
    public void Apply_CombinedFilters_AreAnded()
    {
        var result = PrescriptionSearch.Apply(Sample(), new PrescriptionFilter(Patient: "kai", Outcome: Severity.Low));
        CollectionAssert.AreEqual(new[] { 4 }, Ids(result));
    }

    [TestMethod]
    public void Apply_NoMatches_ReturnsEmptyList()
    {
        var result = PrescriptionSearch.Apply(Sample(), new PrescriptionFilter(Patient: "nobody"));
        Assert.AreEqual(0, result.Count);
    }
}