using MedSafety.Core.Enum;

namespace MedSafety.Core.Models.FormularyTable;

public class InteractionEntry
{
    //Used to show interaction between two specific drug type. For example para + iburprofen, Severity: critical. 
    //Used by Formulary to check if any two types of drugs have clashes.
    public int Id { get; set; }
    public string DrugA { get; set; }
    public string DrugB {get; set;}
    public Severity Severity { get; set; }
}