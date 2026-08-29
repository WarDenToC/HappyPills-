using MedSafety.Core.Enum;

namespace MedSafety.Core.Models.FormularyTable;

public class InteractionEntry
{
    public int Id { get; set; }
    public string DrugA { get; set; }
    public string DrugB {get;set;}
    public Severity Severity { get; set; }
}