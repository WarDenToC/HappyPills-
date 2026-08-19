using MedSafety.Core.Models;
using MedSafety.Core.Rules;

namespace MedSafety.Core;

public class SafetyCheckResult
{
    public IReadOnlyList<Alert> Alerts { get; }
    public Severity Outcome { get; }

    public SafetyCheckResult(IReadOnlyList<Alert> alerts, Severity outcome)
    {
        Alerts = alerts;
        Outcome = outcome;
    }
}

// FR7: overall outcome is the MAX severity across all fired alerts.
// All alerts are returned — the engine does not stop at the first rule that fires.
public class SafetyChecker
{
    private readonly IEnumerable<IPrescriptionRule> _rules;

    public SafetyChecker(IEnumerable<IPrescriptionRule> rules)
    {
        _rules = rules;
    }

    public SafetyCheckResult RunChecks(Prescription prescription, PatientContext patient, Formulary formulary)
    {
        var allAlerts = new List<Alert>();

        foreach (var rule in _rules)
        {
            var alerts = rule.Evaluate(prescription, patient, formulary);
            if (alerts != null)
            {
                allAlerts.AddRange(alerts);
            }
        }

        var outcome = allAlerts.Count > 0
            ? allAlerts.Max(a => a.Severity)
            : Severity.None;

        return new SafetyCheckResult(allAlerts, outcome);
    }
}