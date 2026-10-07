using MedSafety.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MedSafety.Tests;

[TestClass]
public class AccessControlServiceTests
{
    [TestMethod]
    public void Prescriber_CannotReviewPrescriptions()
    {
        Assert.IsFalse(AccessControlService.CanPerform(UserRole.Prescriber, "ReviewPrescription"));
    }

    [TestMethod]
    public void Pharmacist_CanReviewPrescriptions()
    {
        Assert.IsTrue(AccessControlService.CanPerform(UserRole.Pharmacist, "ReviewPrescription"));
    }

    [TestMethod]
    public void Admin_CannotPrescribe()
    {
        Assert.IsFalse(AccessControlService.CanPerform(UserRole.Admin, "Prescribe"));
    }
}