using MedSafety.Core.Audit;
using MedSafety.Core.Security;
using MedSafety.Web.Services;

namespace MedSafety.Tests;

[TestClass]
public class LoginServiceTests
{
    private IMAuditLog _audit = null!;
    private LoginService _login = null!;

    [TestInitialize]
    public void Setup()
    {
        _audit = new IMAuditLog();
        _login = new LoginService(_audit);
    }

    [TestMethod]
    [DataRow(Role.Prescriber, "prescriber")]
    [DataRow(Role.Pharmacist, "pharmacist")]
    [DataRow(Role.Administrator, "admin")]
    public void UsernameFor_ReturnsExpectedUsername(Role role, string expected)
    {
        Assert.AreEqual(expected, LoginService.UsernameFor(role));
    }

    [TestMethod]
    public void TryLogin_CorrectCredentials_SucceedsAndIsAudited()
    {
        var ok = _login.TryLogin(Role.Pharmacist, "pharmacist", "password");

        Assert.IsTrue(ok);
        Assert.AreEqual("LoginSucceeded", _audit.GetAll().Single().Action);
    }

    [TestMethod]
    public void TryLogin_WrongPassword_FailsAndIsAudited()
    {
        var ok = _login.TryLogin(Role.Prescriber, "prescriber", "wrong");

        Assert.IsFalse(ok);
        Assert.AreEqual("LoginFailed", _audit.GetAll().Single().Action);
    }

    [TestMethod]
    public void TryLogin_UsernameDoesNotMatchRole_Fails()
    {
        Assert.IsFalse(_login.TryLogin(Role.Administrator, "prescriber", "password"));
    }

    [TestMethod]
    public void LogOut_WritesAuditEntry()
    {
        _login.LogOut("Dr Prescriber", Role.Prescriber);

        var entry = _audit.GetAll().Single();
        Assert.AreEqual("LogOut", entry.Action);
        Assert.AreEqual(Role.Prescriber, entry.Role);
    }
}