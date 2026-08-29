using MedSafety.Core.Enum;

namespace MedSafety.Core.Models.FormularyTable;

public class DrugEntry
{
    public int ID { get; set; }
    public string DrugName { get; set; }
    public DrugClass DrugClass  { get; set; }
    public DoseUnit DoseUnit { get; set; }

    public int MaxDose { get; set; }
    public int MinDose { get; set; }

    public DrugEntry(int id,DrugClass drugClass, string drugName, DoseUnit doseUnit, int maxDose, int minDose)
    {
        this.ID = id;
        this.DrugClass = drugClass;
        this.DrugName = drugName;
        this.DoseUnit = doseUnit;
        this.MaxDose = maxDose;
        this.MinDose = minDose;
    }
}