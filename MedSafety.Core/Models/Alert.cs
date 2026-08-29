using MedSafety.Core.Enum;

namespace MedSafety.Core.Models;

public class Alert
{
    public Severity Severity { get; set; }
    public RuleType RuleType { get; set; }
    public string Message { get; set; }

    
    public int? Limit { get; set; }
    public int? GivenValue { get; set; }
    public string? RelatedDrug  { get; set; }

    public Alert(Severity severity, RuleType ruleType, string message)
    {
        this.Severity = severity;
        this.RuleType = ruleType;
        this.Message = message;
    }

}