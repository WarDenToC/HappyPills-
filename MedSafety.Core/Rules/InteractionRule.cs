namespace MedSafety.Core.Rules;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;

public class InteractionRule : INterfacePrescriptionRule
{
    public RuleType Type => RuleType.Interaction;

    public IEnumerable<Alert> Check(Prescription prescription, PatientContext patientContext, Formulary formulary)
    {
        List<Alert> alerts = new List<Alert>();
        foreach (string activeMed in patientContext.ActiveMeds)
        {
            var entry = formulary.FindInteraction(prescription.DrugName, activeMed);
            if (entry != null)
            {
                Alert clash = new Alert(entry.Severity, Type,
                    $"{prescription.DrugName} interacts with active medication {activeMed} " +
                    $"(severity: {entry.Severity})");
                clash.RelatedDrug = activeMed;
                alerts.Add(clash);
            }
        }
        return alerts;

    }
}



