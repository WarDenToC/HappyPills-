using MedSafety.Core.Enum;

namespace MedSafety.Web.Services;

public record AlertTypeRow(RuleType RuleType, int Low, int Medium, int Critical)
{
    public int Total => Low + Medium + Critical;
}

public record ReasonCount(string Reason, int Count);

public class DashboardSummary
{
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Approved { get; init; }      // approved without an override
    public int Overridden { get; init; }    // approved with an override reason
    public int Rejected { get; init; }
    public int TotalAlerts { get; init; }
    public double OverrideRatePercent { get; init; }   // overridden / all reviewed
    public double? AverageReviewMinutes { get; init; }
    public IReadOnlyList<AlertTypeRow> AlertsByType { get; init; } = Array.Empty<AlertTypeRow>();
    public IReadOnlyList<ReasonCount> RejectionReasons { get; init; } = Array.Empty<ReasonCount>();
}

// FR14: alerts by type and severity, and rejection reasons (plus override rate and review time)
public static class DashboardMetrics
{
    public static DashboardSummary Calculate(IReadOnlyList<PrescriptionRecord> records)
    {
        var reviewed = records.Where(r => r.Status != "Submitted").ToList();
        var overridden = records.Count(r => r.Status == "Approved" && r.OverrideReason is not null);
        var approved = records.Count(r => r.Status == "Approved" && r.OverrideReason is null);
        var rejected = records.Count(r => r.Status == "Rejected");

        var alerts = records.SelectMany(r => r.SafetyResult.Alerts).ToList();
        var byType = Enum.GetValues<RuleType>()
            .Select(type => new AlertTypeRow(
                type,
                alerts.Count(a => a.RuleType == type && a.Severity == Severity.Low),
                alerts.Count(a => a.RuleType == type && a.Severity == Severity.Medium),
                alerts.Count(a => a.RuleType == type && a.Severity == Severity.Critical)))
            .ToList();

        var reasons = records
            .Where(r => r.Status == "Rejected" && !string.IsNullOrWhiteSpace(r.RejectionReason))
            .GroupBy(r => r.RejectionReason!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new ReasonCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count).ThenBy(x => x.Reason)
            .ToList();

        var turnaround = reviewed
            .Where(r => r.ReviewedAt is not null)
            .Select(r => (r.ReviewedAt!.Value - r.SubmittedAt).TotalMinutes)
            .ToList();

        return new DashboardSummary
        {
            Total = records.Count,
            Pending = records.Count - reviewed.Count,
            Approved = approved,
            Overridden = overridden,
            Rejected = rejected,
            TotalAlerts = alerts.Count,
            OverrideRatePercent = reviewed.Count == 0 ? 0 : overridden * 100.0 / reviewed.Count,
            AverageReviewMinutes = turnaround.Count == 0 ? null : turnaround.Average(),
            AlertsByType = byType,
            RejectionReasons = reasons
        };
    }
}