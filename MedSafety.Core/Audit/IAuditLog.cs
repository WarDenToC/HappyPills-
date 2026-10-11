namespace MedSafety.Core.Audit;

public interface IAuditLog
{
    void Write(AuditEntry entry);
    IReadOnlyList<AuditEntry> GetAll();
}