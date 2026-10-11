using MedSafety.Core.Security;

namespace MedSafety.Core.Audit;

// FR14 / NFR4: one record of something that happened and who did it
public class AuditEntry
{
    public DateTime Timestamp { get; }
    public string Actor { get; }
    public Role Role { get; }
    public string Action { get; }
    public string Detail { get; }

    public AuditEntry(DateTime timestamp, string actor, Role role, string action, string detail)
    {
        Timestamp = timestamp;
        Actor = actor;
        Role = role;
        Action = action;
        Detail = detail;
    }
}