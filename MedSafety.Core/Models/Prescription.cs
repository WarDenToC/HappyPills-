using MedSafety.Core.Enum;
using MedSafety.Core.Models.FormularyTable;

namespace MedSafety.Core.Models;

public class Prescription
{
    public PatientContext PatientContext { get; set; }
    
    public string DrugName  { get; set; }
    public DoseUnit DoseUnit { get; set; }
    public int DoseAmount { get; set; }
    public int Frequency { get; set; }
    public string Duration { get; set; }


    public Prescription(PatientContext patientContext, string drugName,  DoseUnit doseUnit, int doseAmount,int frequency, string duration)
    {
        this.PatientContext = patientContext;
        this.DrugName = drugName;
        this.DoseUnit = doseUnit;
        this.DoseAmount = doseAmount;
        this.Frequency = frequency;
        this.Duration = duration;
    }

}