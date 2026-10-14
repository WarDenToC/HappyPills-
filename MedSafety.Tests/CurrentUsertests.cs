using MedSafety.Core.Security;
using MedSafety.Web.Services;

namespace MedSafety.Tests;

[TestClass]
public class CurrentUserTests
{
    [TestMethod]
    public void NewUser_DefaultsToPrescriber()
    {
        Assert.AreEqual(Role.Prescriber, new CurrentUser().Role);
    }

    [TestMethod]
    public void SwitchTo_DifferentRole_UpdatesRoleAndRaisesChanged()
    {
        var user = new CurrentUser();
        var raised = 0;
        user.Changed += () => raised++;

        user.SwitchTo(Role.Pharmacist);

        Assert.AreEqual(Role.Pharmacist, user.Role);
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void SwitchTo_SameRole_DoesNotRaiseChanged()
    {
        var user = new CurrentUser();
        var raised = 0;
        user.Changed += () => raised++;

        user.SwitchTo(Role.Prescriber);

        Assert.AreEqual(0, raised);
    }
} 