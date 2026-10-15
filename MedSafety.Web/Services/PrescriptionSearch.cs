using MedSafety.Core.Enum;

namespace MedSafety.Web.Services;

public record PrescriptionFilter(
    string? Patient = null,
    string? Status = null,
    Severity? Outcome = null,
    DateTime? From = null,
    DateTime? To = null);

// FR11: search/filter by patient, status, alert severity and date range
public static class PrescriptionSearch
{
    public static List<PrescriptionRecord> Apply(IEnumerable<PrescriptionRecord> records, PrescriptionFilter filter)
    {
        var query = records;

        if (!string.IsNullOrWhiteSpace(filter.Patient))
        {
            var text = filter.Patient.Trim();
            query = query.Where(r =>
                $"{r.Patient.Firstname} {r.Patient.Lastname}".Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            // Approved = approved without an override; Overridden = approved with a recorded override reason
            query = filter.Status switch
            {
                "Overridden" => query.Where(r => r.Status == "Approved" && r.OverrideReason is not null),
                "Approved" => query.Where(r => r.Status == "Approved" && r.OverrideReason is null),
                var s => query.Where(r => r.Status == s)
            };
        }

        if (filter.Outcome is not null)
            query = query.Where(r => r.SafetyResult.Outcome == filter.Outcome);

        if (filter.From is not null)
            query = query.Where(r => r.SubmittedAt.Date >= filter.From.Value.Date);

        if (filter.To is not null)
            query = query.Where(r => r.SubmittedAt.Date <= filter.To.Value.Date);

        return query.OrderByDescending(r => r.SubmittedAt).ThenByDescending(r => r.Id).ToList();
    }
}