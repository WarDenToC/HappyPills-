using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;

namespace MedSafety.Core;

// Partial FR2 validator — currently covers drug name, dose amount, route, and formulary existence.
// Does not yet validate dose unit, frequency, duration, or patient fields.
public static class PrescriptionValidator
{
    public static List<string> Validate(Prescription rx, Formulary formulary)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(rx.DrugName))
        {
            errors.Add("Drug name is required.");
        }

        if (rx.DoseAmount <= 0)
        {
            errors.Add($"Dose amount must be greater than zero (was {rx.DoseAmount}).");
        }

        if (string.IsNullOrWhiteSpace(rx.Route))
        {
            errors.Add("Route is required.");
        }

        if (!string.IsNullOrWhiteSpace(rx.DrugName) && formulary.FindEntry(rx.DrugName) == null)
        {
            errors.Add($"Unknown drug: {rx.DrugName}");
        }

        return errors;
    }
}