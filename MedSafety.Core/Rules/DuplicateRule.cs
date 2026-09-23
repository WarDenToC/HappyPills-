namespace MedSafety.Core.Rules;

using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;
using TypedFormulary = MedSafety.Core.Models.FormularyTable.Formulary;


public class DuplicateRule : INterfacePrescriptionRule
{
    public RuleType Type =>  RuleType.Duplicate;

    public IEnumerable<Alert> Check(Prescription prescription, PatientContext patientContext, TypedFormulary formulary)
    {
        List<Alert> alerts = new List<Alert>();
        var entry = formulary.FindEntry(prescription.DrugName);

        if(entry == null)
            return alerts;

        if (patientContext.ActiveMeds.Contains(prescription.DrugName))
        {
            Alert Duplicatemeds = new Alert(MedSafety.Core.Enum.Severity.Medium, Type,
                $"Patient already has a drug of the same active class {entry.DrugClass}");
        }

        return alerts;
    }

}

/*Medium alert if the patient already has an active drug of the same therapeutic class
(excluding the identical drug if you treat that as a separate "already prescribed" case — decide and document which).*/