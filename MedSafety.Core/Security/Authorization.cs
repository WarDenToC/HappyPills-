using MedSafety.Core.Audit;

namespace MedSafety.Core.Security;

//Decides whether a role may perform an action, and records every refusal
public class AuthorizationService
{
    private readonly IAuditLog _auditLog;

    public AuthorizationService(IAuditLog auditLog)
    {
        _auditLog = auditLog;
    }

    public bool Authorize(string actor, Role role, Permission permission)
    {
        if (IsAllowed(role, permission))
        {
            return true;
        }

        _auditLog.Write(new AuditEntry(DateTime.UtcNow, actor, role, "AccessDenied",
            $"{role} attempted {permission}"));
        return false;
    }

    private static bool IsAllowed(Role role, Permission permission)
    {
        switch (role)
        {
            case Role.Prescriber:
                return permission == Permission.CreatePrescription;
            case Role.Pharmacist:
                return permission == Permission.ReviewPrescription;
            case Role.Administrator:
                return permission == Permission.ViewDashboard;
            default:
                return false;
        }
    }
}