using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MedSafety.Tests;

[TestClass]
public class PrescriptionStoreTests
{
    [TestMethod]
    public void Decide_ApproveWithNoAlerts_Succeeds()
    {
        var store = new PrescriptionStore();
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 50, 1, "Oral", "7 days");
        var patient = new PatientContext(1, "Test", "Patient", new(), new(), new());
        var result = new SafetyCheckResult(new List<Alert>(), Severity.None);
        var record = store.Submit(rx, patient, result);

        var (success, error) = store.Decide(record.Id, "Approved", null);

        Assert.IsTrue(success);
        Assert.IsNull(error);
    }

    [TestMethod]
    public void Decide_ApproveCriticalWithoutReason_IsRefused()
    {
        var store = new PrescriptionStore();
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 600, 1, "Oral", "7 days");
        var patient = new PatientContext(1, "Test", "Patient", new(), new(), new());
        var result = new SafetyCheckResult(
            new List<Alert> { new(Severity.Critical, RuleType.DoseRange, "over max") }, Severity.Critical);
        var record = store.Submit(rx, patient, result);

        var (success, error) = store.Decide(record.Id, "Approved", null);

        Assert.IsFalse(success);
        Assert.IsNotNull(error);
    }

    [TestMethod]
    public void Decide_ApproveCriticalWithReason_SucceedsAndRecordsReason()
    {
        var store = new PrescriptionStore();
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 600, 1, "Oral", "7 days");
        var patient = new PatientContext(1, "Test", "Patient", new(), new(), new());
        var result = new SafetyCheckResult(
            new List<Alert> { new(Severity.Critical, RuleType.DoseRange, "over max") }, Severity.Critical);
        var record = store.Submit(rx, patient, result);

        var (success, error) = store.Decide(record.Id, "Approved", "Clinically justified per specialist consult.");

        Assert.IsTrue(success);
        Assert.AreEqual("Approved", record.Status);
        Assert.AreEqual("Clinically justified per specialist consult.", record.OverrideReason);
    }
}