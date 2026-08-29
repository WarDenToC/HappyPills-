using MedSafety.Core.Enum;
using MedSafety.Core.Models.FormularyTable;

namespace MedSafety.Core.Models;

public class Prescription
{
    public PatientContext PatientContext { get; set; }
    
    public string DrugName  { get; set; }
    public DoseUnit DoseUnit { get; set; }
    public int Frequency { get; set; }
    public string Duration { get; set; }


    public Prescription(PatientContext patientContext, string drugName,  DoseUnit doseUnit, int frequency, string duration)
    {
        this.PatientContext = patientContext;
        this.DrugName = drugName;
        this.DoseUnit = doseUnit;
        this.Frequency = frequency;
        this.Duration = duration;
    }

}