using MedSafety.Core.Audit;
using MedSafety.Data.Rows;
using Microsoft.EntityFrameworkCore;

namespace MedSafety.Data;

// Same interface as IMAuditLog, so Authorization doesn't change at all
public class SqliteAuditLog : IAuditLog
{
    private readonly IDbContextFactory<MedSafetyDbContext> _factory;

    public SqliteAuditLog(IDbContextFactory<MedSafetyDbContext> factory)
    {
        _factory = factory;
    }

    public void Write(AuditEntry entry)
    {
        using var db = _factory.CreateDbContext();
        db.AuditEntries.Add(new AuditRow
        {
            Timestamp = entry.Timestamp,
            Actor = entry.Actor,
            Role = entry.Role,
            Action = entry.Action,
            Detail = entry.Detail
        });
        db.SaveChanges();
    }

    public IReadOnlyList<AuditEntry> GetAll()
    {
        using var db = _factory.CreateDbContext();
        return db.AuditEntries.OrderBy(a => a.Id).ToList()
            .Select(a => new AuditEntry(a.Timestamp, a.Actor, a.Role, a.Action, a.Detail))
            .ToList();
    }
}