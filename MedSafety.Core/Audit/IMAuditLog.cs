namespace MedSafety.Core.Audit;

public class IMAuditLog : IAuditLog
{
    private readonly List<AuditEntry> _entries = new();

    public void Write(AuditEntry entry)
    {
        _entries.Add(entry);
    }

    public IReadOnlyList<AuditEntry> GetAll()
    {
        return _entries;
    }
}