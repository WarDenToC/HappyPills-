using MedSafety.Core.Enum;

namespace MedSafety.Core.Models;

// FR8: every alert must state which rule fired, the values involved, and a human-readable reason
public class Alert
{
    public string RuleName { get; }
    public Severity Severity { get; }
    public string ViolatedValues { get; }
    public string Reason { get; }
    public RuleType? RuleType { get; }
    public string Message => Reason;
    public string? RelatedDrug { get; set; }

    public Alert(string ruleName, Severity severity, string violatedValues, string reason)
    {
        RuleName = ruleName;
        Severity = severity;
        ViolatedValues = violatedValues;
        Reason = reason;
    }

    public Alert(MedSafety.Core.Enum.Severity severity, RuleType ruleType, string message)
    {
        RuleName = ruleType.ToString();
        Severity = (Severity)(int)severity;
        ViolatedValues = string.Empty;
        Reason = message;
        RuleType = ruleType;
    }
}