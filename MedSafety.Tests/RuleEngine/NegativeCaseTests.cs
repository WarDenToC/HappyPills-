using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;
using MedSafety.Core.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MedSafety.Tests.RuleEngine;

[TestClass]
public class NegativeCaseTests
{
    private static Formulary BuildFormulary() => new(
        new List<DrugEntry>
        {
            new(1, "Alphacillin", DrugClass.Penicilin, DoseUnit.Mg, 250, 500),
            new(2, "Betanol", DrugClass.BetaBlocker, DoseUnit.Mg, 25, 100),
            new(3, "Dolorex", DrugClass.NSAID, DoseUnit.Mg, 500, 1000)
        },
        new List<InteractionEntry>());

    [TestMethod]
    public void Allergy_PatientWithNoAllergies_RaisesNoAlert()
    {
        var patient = new PatientContext(1, "Test", "Patient", new(), new(), new());
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 300, 1, "Oral", "7 days");

        var alerts = new AllergyRule().Evaluate(rx, patient, BuildFormulary());

        Assert.AreEqual(0, alerts.Count());
    }

    [TestMethod]
    public void Duplicate_ActiveDrugDifferentClass_RaisesNoAlert()
    {
        var patient = new PatientContext(1, "Test", "Patient",
            new(), new(), new List<string> { "Dolorex" }); // NSAID, different class
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 300, 1, "Oral", "7 days"); // Penicilin

        var alerts = new DuplicateRule().Evaluate(rx, patient, BuildFormulary());

        Assert.AreEqual(0, alerts.Count());
    }
}