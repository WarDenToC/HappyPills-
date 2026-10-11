using MedSafety.Core.Audit;
using MedSafety.Core.Security;

namespace MedSafety.Tests;

[TestClass]
public class AuthorizationServiceTests
{
    private IMAuditLog _auditLog = null!;
    private AuthorizationService _authorizationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _auditLog = new IMAuditLog();
        _authorizationService = new AuthorizationService(_auditLog);
    }

    [TestMethod]
    public void Authorize_PrescriberReviewPrescription_Denied()
    {
        var result = _authorizationService.Authorize("Alice", Role.Prescriber, Permission.ReviewPrescription);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Authorize_PrescriberReviewPrescription_WritesOneAuditEntry()
    {
        var actor = "Alice";
        var role = Role.Prescriber;

        _authorizationService.Authorize(actor, role, Permission.ReviewPrescription);

        Assert.AreEqual(1, _auditLog.GetAll().Count);
        var entry = _auditLog.GetAll()[0];
        Assert.AreEqual(actor, entry.Actor);
        Assert.AreEqual(role, entry.Role);
        Assert.AreEqual("AccessDenied", entry.Action);
        StringAssert.Contains(entry.Detail, "ReviewPrescription");
    }

    [TestMethod]
    public void Authorize_PharmacistReviewPrescription_Allowed()
    {
        var result = _authorizationService.Authorize("Bob", Role.Pharmacist, Permission.ReviewPrescription);
        Assert.IsTrue(result);
        Assert.AreEqual(0, _auditLog.GetAll().Count);
    }

    [TestMethod]
    [DataRow(Role.Prescriber, Permission.CreatePrescription, true)]
    [DataRow(Role.Prescriber, Permission.ReviewPrescription, false)]
    [DataRow(Role.Prescriber, Permission.ViewDashboard, false)]
    [DataRow(Role.Pharmacist, Permission.CreatePrescription, false)]
    [DataRow(Role.Pharmacist, Permission.ReviewPrescription, true)]
    [DataRow(Role.Pharmacist, Permission.ViewDashboard, false)]
    [DataRow(Role.Administrator, Permission.CreatePrescription, false)]
    [DataRow(Role.Administrator, Permission.ReviewPrescription, false)]
    [DataRow(Role.Administrator, Permission.ViewDashboard, true)]
    public void Authorize_AllCombinations_ReturnValue(Role role, Permission permission, bool expected)
    {
        var result = _authorizationService.Authorize("User", role, permission);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow(Role.Prescriber, Permission.CreatePrescription, 0)]
    [DataRow(Role.Prescriber, Permission.ReviewPrescription, 1)]
    [DataRow(Role.Prescriber, Permission.ViewDashboard, 1)]
    [DataRow(Role.Pharmacist, Permission.CreatePrescription, 1)]
    [DataRow(Role.Pharmacist, Permission.ReviewPrescription, 0)]
    [DataRow(Role.Pharmacist, Permission.ViewDashboard, 1)]
    [DataRow(Role.Administrator, Permission.CreatePrescription, 1)]
    [DataRow(Role.Administrator, Permission.ReviewPrescription, 1)]
    [DataRow(Role.Administrator, Permission.ViewDashboard, 0)]
    public void Authorize_AllCombinations_AuditLogCount(Role role, Permission permission, int expectedCount)
    {
        _authorizationService.Authorize("User", role, permission);
        Assert.AreEqual(expectedCount, _auditLog.GetAll().Count);
    }

    [TestMethod]
    public void IMAuditLog_WriteMultipleEntries_ReturnsInOrder()
    {
        var entry1 = new AuditEntry(DateTime.UtcNow, "Actor1", Role.Prescriber, "Action1", "Detail1");
        var entry2 = new AuditEntry(DateTime.UtcNow, "Actor2", Role.Pharmacist, "Action2", "Detail2");

        _auditLog.Write(entry1);
        _auditLog.Write(entry2);

        var all = _auditLog.GetAll();
        Assert.AreEqual(2, all.Count);
        Assert.AreEqual("Actor1", all[0].Actor);
        Assert.AreEqual("Actor2", all[1].Actor);
    }
}
