using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;
using TypedFormulary = MedSafety.Core.Models.FormularyTable.Formulary;

namespace MedSafety.Core.Rules;

public interface INterfacePrescriptionRule
{
    RuleType Type { get; }
    IEnumerable<Alert> Check(Prescription prescription, PatientContext patientContext, TypedFormulary formulary);
}
