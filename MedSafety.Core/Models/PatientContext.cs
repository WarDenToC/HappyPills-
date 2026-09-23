namespace MedSafety.Core.Models;

// Supports FR4 (allergy), FR5 (interaction), FR6 (duplicate therapy).
public class PatientContext
{
    public List<string> Allergies { get; set; } = new();
    public List<string> ActiveMedications { get; set; } = new();
    public int PatientId { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public List<string> DrugAllergies { get; set; } = new();
    public List<string> ClassAllergies { get; set; } = new();
    public List<string> ActiveMeds { get; set; } = new();

    public PatientContext()
    {
    }

    public PatientContext(
        int patientId,
        string firstname,
        string lastname,
        List<string> drugAllergies,
        List<string> classAllergies,
        List<string> activeMeds)
    {
        PatientId = patientId;
        Firstname = firstname;
        Lastname = lastname;
        DrugAllergies = drugAllergies;
        ClassAllergies = classAllergies;
        ActiveMeds = activeMeds;
        Allergies = drugAllergies;
        ActiveMedications = activeMeds;
    }
}