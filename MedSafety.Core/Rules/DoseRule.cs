using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;

namespace MedSafety.Core.Rules;

public class DoseRangeRule : INterfacePrescriptionRule
{
    public RuleType Type =>  RuleType.DoseRange;
    public IEnumerable<Alert> Check(Prescription prescription, PatientContext patientContext, Formulary formulary)
    {
        List<Alert> alerts = new List<Alert>();
        var entry = formulary.FindEntry(prescription.DrugName);

        if (entry == null)
            return alerts;

        if (prescription.DoseUnit != entry.DoseUnit)
        {
            Alert unitMismatch = new Alert(Severity.Critical, Type,
                $"Dose unit {prescription.DoseUnit} does not match formulary unit " +
                $"{entry.DoseUnit} for {entry.DrugName}; dose cannot be checked");
            alerts.Add(unitMismatch);
            return alerts;
        }
        
        if (prescription.DoseAmount < entry.MinDose)
        {
            Alert underDose = new Alert(Severity.Low, Type, 
                $"Dose {prescription.DoseAmount}{prescription.DoseUnit} " +
                $"is below the required dose of " +
                $"{entry.MinDose}{entry.DoseUnit}");
            alerts.Add(underDose);
        }

        else if (prescription.DoseAmount > entry.MaxDose)
        {
            Alert overDose = new Alert(Severity.Critical, Type,
                $"Dose {prescription.DoseAmount}{prescription.DoseUnit} " +
                $"is above the required dose of " +
                $"{entry.MaxDose}{entry.DoseUnit}");
            alerts.Add(overDose);

        }

        return alerts;
    }
}

