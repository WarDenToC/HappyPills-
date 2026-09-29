using MedSafety.Core.Enum;

namespace MedSafety.Core.Models.FormularyTable;

public class DrugEntry
{
    // This
    public int ID { get; set; }
    public string DrugName { get; set; }
    public DrugClass DrugClass  { get; set; }
    public DoseUnit DoseUnit { get; set; }

    public decimal MaxDose { get; set; }
    public decimal MinDose { get; set; }

    public DrugEntry(int id, string drugName, DrugClass drugClass, DoseUnit doseUnit, decimal minDose, decimal maxDose)
    {
        ID = id;
        DrugName = drugName;
        DrugClass = drugClass;
        DoseUnit = doseUnit;
        MinDose = minDose;
        MaxDose = maxDose;
    }
}