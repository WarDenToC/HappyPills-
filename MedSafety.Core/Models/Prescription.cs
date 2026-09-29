using MedSafety.Core.Enum;

namespace MedSafety.Core.Models;

public class Prescription
{
    public int PatientId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public DoseUnit DoseUnit { get; set; }
    public decimal DoseAmount { get; set; }
    public int Frequency { get; set; }          // times per day
    public string Route { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;

    // Needed by tests (new Prescription()) and the Blazor form
    public Prescription() { }

    public Prescription(int patientId, string drugName, DoseUnit doseUnit,
        decimal doseAmount, int frequency, string route, string duration)
    {
        PatientId = patientId;
        DrugName = drugName;
        DoseUnit = doseUnit;
        DoseAmount = doseAmount;
        Frequency = frequency;
        Route = route;
        Duration = duration;
    }
}