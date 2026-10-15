using MedSafety.Core;
using MedSafety.Core.Audit;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Security;
using MedSafety.Web.Services;

namespace MedSafety.Tests;

[TestClass]
public class PrescriptionWorkflowTests
{
    private IMAuditLog _audit = null!;
    private PrescriptionStore _store = null!;
    private PrescriptionWorkflow _workflow = null!;

    [TestInitialize]
    public void Setup()
    {
        _audit = new IMAuditLog();
        _store = new PrescriptionStore();
        _workflow = new PrescriptionWorkflow(_store, new AuthorizationService(_audit), _audit);
    }

    private static Prescription Rx() => new(1, "Alphacillin", DoseUnit.Mg, 600, 1, "Oral", "7 days");
    private static PatientContext Patient() => new(1, "Test", "Patient", new(), new(), new());

    private static SafetyCheckResult CriticalResult() =>
        new(new List<Alert> { new(Severity.Critical, RuleType.DoseRange, "over max") }, Severity.Critical);

    [TestMethod]
    public void Submit_AsPrescriber_StoresRecordAndAuditsSubmissionAndEachAlert()
    {
        var record = _workflow.Submit("Dr Prescriber", Role.Prescriber, Rx(), Patient(), CriticalResult());

        Assert.IsNotNull(record);
        Assert.AreEqual(1, _store.All.Count);
        var entries = _audit.GetAll();
        Assert.AreEqual(2, entries.Count);
        Assert.AreEqual("PrescriptionSubmitted", entries[0].Action);
        Assert.AreEqual("AlertRaised", entries[1].Action);
    }

    [TestMethod]
    public void Submit_AsPharmacist_IsDeniedAndAudited()
    {
        var record = _workflow.Submit("Pat Pharmacist", Role.Pharmacist, Rx(), Patient(), CriticalResult());

        Assert.IsNull(record);
        Assert.AreEqual(0, _store.All.Count);
        Assert.AreEqual(1, _audit.GetAll().Count);
        Assert.AreEqual("AccessDenied", _audit.GetAll()[0].Action);
    }

    [TestMethod]
    public void Review_AsPrescriber_IsDeniedAuditedAndLeavesStatusUnchanged()
    {
        var record = _workflow.Submit("Dr Prescriber", Role.Prescriber, Rx(), Patient(), CriticalResult())!;

        var (success, error) = _workflow.Review("Dr Prescriber", Role.Prescriber, record.Id, "Approved", "because");

        Assert.IsFalse(success);
        StringAssert.Contains(error, "AccessDenied");
        Assert.AreEqual("Submitted", record.Status);
        Assert.AreEqual("AccessDenied", _audit.GetAll().Last().Action);
    }

    [TestMethod]
    public void Review_CriticalApprovedWithReason_IsAuditedAsOverrideWithReason()
    {
        var record = _workflow.Submit("Dr Prescriber", Role.Prescriber, Rx(), Patient(), CriticalResult())!;

        var (success, _) = _workflow.Review("Pat Pharmacist", Role.Pharmacist, record.Id, "Approved", "Specialist consult");

        Assert.IsTrue(success);
        var last = _audit.GetAll().Last();
        Assert.AreEqual("OverrideApproved", last.Action);
        StringAssert.Contains(last.Detail, "Specialist consult");
    }

    [TestMethod]
    public void Review_RejectWithoutReason_IsRefusedAndStatusUnchanged()
    {
        var record = _workflow.Submit("Dr Prescriber", Role.Prescriber, Rx(), Patient(), CriticalResult())!;

        var (success, error) = _workflow.Review("Pat Pharmacist", Role.Pharmacist, record.Id, "Rejected", "  ");

        Assert.IsFalse(success);
        Assert.IsNotNull(error);
        Assert.AreEqual("Submitted", record.Status);
    }

    [TestMethod]
    public void Review_RejectWithReason_RecordsReasonAndAudits()
    {
        var record = _workflow.Submit("Dr Prescriber", Role.Prescriber, Rx(), Patient(), CriticalResult())!;

        var (success, _) = _workflow.Review("Pat Pharmacist", Role.Pharmacist, record.Id, "Rejected", "Dose too high");

        Assert.IsTrue(success);
        Assert.AreEqual("Rejected", record.Status);
        Assert.AreEqual("Dose too high", record.RejectionReason);
        Assert.AreEqual("Rejected", _audit.GetAll().Last().Action);
    }

    [TestMethod]
    public void Review_SecondDecisionOnSameRecord_IsRefused()
    {
        var record = _workflow.Submit("Dr Prescriber", Role.Prescriber, Rx(), Patient(), CriticalResult())!;
        _workflow.Review("Pat Pharmacist", Role.Pharmacist, record.Id, "Rejected", "Dose too high");

        var (success, error) = _workflow.Review("Pat Pharmacist", Role.Pharmacist, record.Id, "Approved", "changed mind");

        Assert.IsFalse(success);
        StringAssert.Contains(error, "already been reviewed");
    }
}