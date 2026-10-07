using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;

// Synthetic data only, still waiting on real seed data once MedSafety.data lands

namespace MedSafety.Web.Services;

public static class SampleData
{
    public static Formulary Formulary { get; } = new(
        new List<DrugEntry>
        {
            new(1, "Alphacillin", DrugClass.Penicilin, DoseUnit.Mg, 250, 500),
            new(2, "Betanol", DrugClass.BetaBlocker, DoseUnit.Mg, 25, 100),
            new(3, "Cardizyme", DrugClass.BetaBlocker, DoseUnit.Mg, 5, 20),
            new(4, "Dolorex", DrugClass.NSAID, DoseUnit.Mg, 500, 1000)
        },
        new List<InteractionEntry>
        {
            new() { Id = 1, DrugA = "Dolorex", DrugB = "Cardizyme", Severity = Severity.Critical }
        });

    public static List<PatientContext> Patients { get; } = new()
    {
        new PatientContext(1, "Kai", "Ngata",
            new List<string> { "alphacillin" }, new List<string>(), new List<string>()),
        new PatientContext(2, "Riya", "Sharma",
            new List<string>(), new List<string>(), new List<string> { "Cardizyme" }),
        new PatientContext(3, "Noah", "Fletcher",
            new List<string>(), new List<string>(), new List<string> { "Betanol" })
    };
}