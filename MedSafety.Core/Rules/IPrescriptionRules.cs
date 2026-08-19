using MedSafety.Core.Models;

namespace MedSafety.Core.Rules;

// NFR7: new rules are added by implementing this interface, without touching existing rules
public interface IPrescriptionRule
{
    string Name { get; }
    IEnumerable<Alert> Evaluate(Prescription prescription, PatientContext patient, Formulary formulary);
}