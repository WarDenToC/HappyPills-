using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;

namespace MedSafety.Core;

// FR2: reject incomplete or invalid prescriptions before any rule runs
public static class PrescriptionValidator
{
    public static List<string> Validate(Prescription prescription, Formulary formulary)
    {
        var errors = new List<string>();
        var entry = formulary.FindEntry(prescription.DrugName);
        // TODO (Amrith): drug name, route, duration not empty; dose > 0; frequency > 0; drug exists in formulary

        if (string.IsNullOrWhiteSpace(prescription.DrugName))
        {
            String emptyName = "Drug name is required.";
            errors.Add(emptyName);
        }
        else if (formulary.FindEntry(prescription.DrugName) == null)
        {
            String drugNonExist = $"Drug '{prescription.DrugName}' was not found in the formulary.";
            errors.Add(drugNonExist);
        }

        if (string.IsNullOrWhiteSpace(prescription.Route))
        {
            String emptyRoute = "Please ensure that this field of intake route is filled";
            errors.Add(emptyRoute);
        }

        if (string.IsNullOrWhiteSpace(prescription.Duration))
        {
            String emptyDuration = "Please ensure that the duration of prescription is filled";
            errors.Add(emptyDuration);
        }

        if (prescription.DoseAmount <= 0)
        {
            String zeroDoseAmount = "Please ensure that the dose amount is greater than zero";
            errors.Add(zeroDoseAmount);
        }

        if (prescription.Frequency <= 0)
        {
            String zeroFrequency = "Please ensure that the frequency is filled and is greater than zero";
            errors.Add(zeroFrequency);
        }
        
        return errors;
    }
}