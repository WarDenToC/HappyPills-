using MedSafety.Core.Audit;
using MedSafety.Core.Security;

namespace MedSafety.Web.Services;

// Prototype sign-in: one shared demo password. Every attempt is written to the audit log.
// A production system would use hashed credentials (e.g. ASP.NET Core Identity).
public class LoginService
{
    public const string DemoPassword = "password";

    private readonly IAuditLog _audit;

    public LoginService(IAuditLog audit)
    {
        _audit = audit;
    }

    public static string UsernameFor(Role role) => role switch
    {
        Role.Prescriber => "prescriber",
        Role.Pharmacist => "pharmacist",
        Role.Administrator => "admin",
        _ => ""
    };

    public bool TryLogin(Role role, string? username, string? password)
    {
        var success = string.Equals(username, UsernameFor(role), StringComparison.Ordinal)
                      && password == DemoPassword;

        var actor = string.IsNullOrWhiteSpace(username) ? "(blank)" : username;
        _audit.Write(new AuditEntry(DateTime.UtcNow, actor, role,
            success ? "LoginSucceeded" : "LoginFailed",
            success ? $"{role} signed in" : $"Failed sign-in attempt as {role}"));

        return success;
    }

    public void LogOut(string actor, Role role) =>
        _audit.Write(new AuditEntry(DateTime.UtcNow, actor, role, "LogOut", $"{role} signed out"));
}