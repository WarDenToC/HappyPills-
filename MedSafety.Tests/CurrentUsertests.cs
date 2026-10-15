using MedSafety.Core.Security;
using MedSafety.Web.Services;

namespace MedSafety.Tests;

[TestClass]
public class CurrentUserTests
{
    [TestMethod]
    public void NewUser_IsNotAuthenticated()
    {
        Assert.IsFalse(new CurrentUser().IsAuthenticated);
    }

    [TestMethod]
    public void SignIn_SetsRoleAndRaisesChanged()
    {
        var user = new CurrentUser();
        var raised = 0;
        user.Changed += () => raised++;

        user.SignIn(Role.Pharmacist);

        Assert.IsTrue(user.IsAuthenticated);
        Assert.AreEqual(Role.Pharmacist, user.Role);
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void SignOut_ClearsAuthenticationAndRaisesChanged()
    {
        var user = new CurrentUser();
        user.SignIn(Role.Administrator);
        var raised = 0;
        user.Changed += () => raised++;

        user.SignOut();

        Assert.IsFalse(user.IsAuthenticated);
        Assert.AreEqual(1, raised);
    }
}