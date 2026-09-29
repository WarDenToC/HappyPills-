namespace MedSafety.Core.Rules;

using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;


public class DuplicateRule : INterfacePrescriptionRule
{
    public RuleType Type =>  RuleType.Duplicate;
    
    public IEnumerable<Alert> Check(Prescription prescription, PatientContext patientContext, Formulary formulary)
    {
        List<Alert> alerts = new List<Alert>();
        var entry = formulary.FindEntry(prescription.DrugName);
        
        if (entry == null)
            return alerts;
        
        foreach (string activeMed in patientContext.ActiveMeds)
        {
            var activeEntry = formulary.FindEntry(activeMed);
            
            if (activeEntry == null)
                continue;
            
            if (entry.DrugName.Equals(activeEntry.DrugName))
            {
                Alert duplicateDrug = new Alert(Severity.Medium, Type,
                    $"Patient already has the same active drug of {activeEntry.DrugName}");
                alerts.Add(duplicateDrug);
            }

            else if (entry.DrugClass.Equals(activeEntry.DrugClass))
            {
                Alert duplicateClass = new Alert(Severity.Low, Type,
                    $"Patient already has the same active drug class of {activeEntry.DrugName}");
                alerts.Add(duplicateClass);
            }
        }

        return alerts;
    }

}

/*Medium alert if the patient already has an active drug of the same therapeutic class
(excluding the identical drug if you treat that as a separate "already prescribed" case — decide and document which).*/