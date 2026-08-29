using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;

namespace MedSafety.Core.Rules;

public interface INterfacePrescriptionRule
{
    RuleType Type { get; }
    IEnumerable<Alert> Check(Prescription prescription, PatientContext patientContext, Formulary formulary);
}
