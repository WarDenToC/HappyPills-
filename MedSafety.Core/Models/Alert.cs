using MedSafety.Core.Enum;

namespace MedSafety.Core.Models;

public class Alert
{
    /*
     * All alerts must have the severity that they get that from the Enum Severity
     * Then it must alert what type of rules is being violated from the list of rules from Enum
     * Then finally it will print message of what values or any type of variables that's being violated 
     */ 
    public Severity Severity { get; set; }
    public RuleType RuleType { get; set; }
    public string Message { get; set; }
    
    public string? RelatedDrug  { get; set; } //currently not implemented but will be used for interaction clash or duplicate drug rules

    public Alert(Severity severity, RuleType ruleType, string message)
    {
        this.Severity = severity;
        this.RuleType = ruleType;
        this.Message = message;
    }

}