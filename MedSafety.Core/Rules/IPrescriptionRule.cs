using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;

namespace MedSafety.Core.Rules;


public interface IPrescriptionRule
{
    RuleType Type { get; }
    IEnumerable<Alert> Evaluate(Prescription prescription, PatientContext patient, Formulary formulary);
}