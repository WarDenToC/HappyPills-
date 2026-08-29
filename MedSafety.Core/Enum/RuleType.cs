namespace MedSafety.Core.Enum;

public enum RuleType
{
    // will be used by Alert to fire off what kind of rule was being violated 
    DoseRange,
    Interaction,
    Duplicate,
    Allergies
}