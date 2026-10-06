using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;
using MedSafety.Core.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MedSafety.Tests.RuleEngine;

// These tests document known gaps (see COPILOT-LOG.md / defect log D-02, D-03).
// They are EXPECTED TO FAIL until the corresponding Core fix lands — do not "fix" the test to pass.
[TestClass]
public class KnownDefectsTests
{
    private static Formulary BuildFormulary() => new(
        new List<DrugEntry> { new(1, "Alphacillin", DrugClass.Penicilin, DoseUnit.Mg, 250, 500) },
        new List<InteractionEntry>());

    private static SafetyChecker BuildChecker() => new(new IPrescriptionRule[]
    {
        new DoseRangeRule(), new AllergyRule(), new InteractionRule(), new DuplicateRule()
    });

    [TestMethod]
    public void Issue1_Allergy_CaseInsensitiveMatch_ShouldRaiseCriticalAlert()
    {
        // FR4 requires case-insensitive matching. AllergyRule currently uses Contains(),
        // which is case-sensitive — "alphacillin" (stored) vs "Alphacillin" (prescribed) won't match.
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string> { "alphacillin" }, new List<string>(), new List<string>());
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 300, 1, "Oral", "7 days");

        var alerts = new AllergyRule().Evaluate(rx, patient, BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Critical, alerts[0].Severity);
    }

    [TestMethod]
    public void Issue2_UnknownDrug_ThroughSafetyChecker_ShouldBeInvalidWithNoAlerts()
    {
        // SafetyChecker has no concept yet of "this drug isn't in the formulary at all" —
        // IsValid is currently hardcoded true regardless of input.
        var rx = new Prescription(1, "PTY1", DoseUnit.Mg, 100, 1, "Oral", "7 days");
        var patient = new PatientContext(1, "Test", "Patient", new(), new(), new());

        var result = BuildChecker().RunChecks(rx, patient, BuildFormulary());

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(0, result.Alerts.Count);
    }
}