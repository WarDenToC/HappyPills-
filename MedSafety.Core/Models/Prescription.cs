namespace MedSafety.Core.Models;

// FR1: the 7 mandatory fields. Stub for now — partner may expand with IDs, status enum, etc.
public class Prescription
{
    public string DrugName { get; set; } = string.Empty;
    public double DoseAmount { get; set; }
    public string DoseUnit { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
}