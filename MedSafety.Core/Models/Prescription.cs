namespace MedSafety.Core.Models;

// FR1: the 7 mandatory fields, with typed formulary data available when needed.
public class Prescription
{
    public PatientContext? PatientContext { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public double DoseAmount { get; set; }
    public string DoseUnit { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;

    public Prescription()
    {
    }

    public Prescription(
        PatientContext patientContext,
        string drugName,
        MedSafety.Core.Enum.DoseUnit doseUnit,
        int doseAmount,
        int frequency,
        string duration)
    {
        PatientContext = patientContext;
        DrugName = drugName;
        DoseUnit = doseUnit.ToString();
        DoseAmount = doseAmount;
        Frequency = frequency.ToString();
        Duration = duration;
    }
}
