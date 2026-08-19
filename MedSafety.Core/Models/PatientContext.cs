namespace MedSafety.Core.Models;

// Supports FR4 (allergy), FR5 (interaction), FR6 (duplicate therapy). Stub — expand with patient ID etc. later.
public class PatientContext
{
    public List<string> Allergies { get; set; } = new();
    public List<string> ActiveMedications { get; set; } = new();
}