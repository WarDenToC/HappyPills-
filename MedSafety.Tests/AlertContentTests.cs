using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;
using MedSafety.Core.Rules;

namespace MedSafety.Tests;

[TestClass]
public class AlertContentTests
{
    private static Formulary BuildFormulary() => new(
        new List<DrugEntry>
        {
            new(1, "Alphacillin", DrugClass.Penicilin, DoseUnit.Mg, 250, 500),
            new(2, "Cardizyme", DrugClass.BetaBlocker, DoseUnit.Mg, 5, 20),
            new(3, "Dolorex", DrugClass.NSAID, DoseUnit.Mg, 500, 1000)
        },
        new List<InteractionEntry>
        {
            new() { Id = 1, DrugA = "Dolorex", DrugB = "Cardizyme", Severity = Severity.Critical }
        });

    [TestMethod]
    public void DoseAlert_StatesRuleAndBothComparedValues()
    {
        var patient = new PatientContext(1, "Test", "Patient", new(), new(), new());
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 600, 1, "Oral", "7 days");

        var alert = new DoseRangeRule().Evaluate(rx, patient, BuildFormulary()).Single();

        Assert.AreEqual(RuleType.DoseRange, alert.RuleType);
        StringAssert.Contains(alert.Message, "600");
        StringAssert.Contains(alert.Message, "500");
    }

    [TestMethod]
    public void InteractionAlert_NamesBothDrugsAndTheActiveMedication()
    {
        var patient = new PatientContext(1, "Test", "Patient", new(), new(), new List<string> { "Cardizyme" });
        var rx = new Prescription(1, "Dolorex", DoseUnit.Mg, 750, 1, "Oral", "7 days");

        var alert = new InteractionRule().Evaluate(rx, patient, BuildFormulary()).Single();

        Assert.AreEqual(RuleType.Interaction, alert.RuleType);
        StringAssert.Contains(alert.Message, "Dolorex");
        StringAssert.Contains(alert.Message, "Cardizyme");
        Assert.AreEqual("Cardizyme", alert.RelatedDrug);
    }

    [TestMethod]
    public void AllergyAlert_NamesTheDrug()
    {
        var patient = new PatientContext(1, "Test", "Patient", new List<string> { "alphacillin" }, new(), new());
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 300, 1, "Oral", "7 days");

        var alert = new AllergyRule().Evaluate(rx, patient, BuildFormulary()).Single();

        Assert.AreEqual(RuleType.Allergies, alert.RuleType);
        StringAssert.Contains(alert.Message, "Alphacillin");
    }
}