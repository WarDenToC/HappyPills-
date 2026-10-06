using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;
using MedSafety.Core.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MedSafety.Tests.RuleEngine;

[TestClass]
public class SafetyCheckerTests
{
    // Synthetic formulary — fictional drugs only, per project brief (§0)
    private static Formulary BuildFormulary()
    {
        var entries = new List<DrugEntry>
        {
            new(1, "Alphacillin", DrugClass.Penicilin, DoseUnit.Mg, 250, 500),
            new(2, "Betanol", DrugClass.BetaBlocker, DoseUnit.Mg, 25, 100),
            new(3, "Cardizyme", DrugClass.BetaBlocker, DoseUnit.Mg, 5, 20),
            new(4, "Dolorex", DrugClass.NSAID, DoseUnit.Mg, 500, 1000)
        };

        var interactions = new List<InteractionEntry>
        {
            new() { Id = 1, DrugA = "Dolorex", DrugB = "Cardizyme", Severity = Severity.Critical }
        };

        return new Formulary(entries, interactions);
    }

    private static PatientContext CleanPatient() =>
        new(1, "Test", "Patient", new List<string>(), new List<string>(), new List<string>());

    private static SafetyChecker BuildChecker() => new(new IPrescriptionRule[]
    {
        new DoseRangeRule(),
        new AllergyRule(),
        new InteractionRule(),
        new DuplicateRule()
    });

    // ---------- FR1/FR7 — happy path ----------
    [TestMethod]
    public void TC1_HappyPath_NoAllergiesNoActiveMeds_ReturnsNoneOutcome()
    {
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 50, 1, "Oral", "7 days");
        var result = BuildChecker().RunChecks(rx, CleanPatient(), BuildFormulary());

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(Severity.None, result.Outcome);
        Assert.AreEqual(0, result.Alerts.Count);
    }

    // ---------- FR3 — dose range boundaries ----------
    [TestMethod]
    public void TC2_DoseRange_ExactlyAtMinimum_IsAllowed()
    {
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 25, 1, "Oral", "7 days");
        var alerts = new DoseRangeRule().Evaluate(rx, CleanPatient(), BuildFormulary());
        Assert.AreEqual(0, alerts.Count());
    }

    [TestMethod]
    public void TC3_DoseRange_ExactlyAtMaximum_IsAllowed()
    {
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 100, 1, "Oral", "7 days");
        var alerts = new DoseRangeRule().Evaluate(rx, CleanPatient(), BuildFormulary());
        Assert.AreEqual(0, alerts.Count());
    }

    [TestMethod]
    public void TC4_DoseRange_JustOverMaximum_RaisesCriticalAlert()
    {
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 101, 1, "Oral", "7 days");
        var alerts = new DoseRangeRule().Evaluate(rx, CleanPatient(), BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Critical, alerts[0].Severity);
    }

    [TestMethod]
    public void TC5_DoseRange_JustUnderMinimum_RaisesLowAlert()
    {
        // Current implementation raises Low for sub-therapeutic dose (matches current skeleton FR3: Low/Medium/Critical)
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 24, 1, "Oral", "7 days");
        var alerts = new DoseRangeRule().Evaluate(rx, CleanPatient(), BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Low, alerts[0].Severity);
    }

    [TestMethod]
    public void TC6_DoseRange_UnitMismatch_RaisesCriticalGuardAlert()
    {
        // Implementation-specific guard beyond the minimum FR3 requirement — dose can't be safely compared across units
        var rx = new Prescription(1, "Betanol", DoseUnit.G, 50, 1, "Oral", "7 days");
        var alerts = new DoseRangeRule().Evaluate(rx, CleanPatient(), BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Critical, alerts[0].Severity);
    }

    // ---------- FR4 — allergy ----------
    [TestMethod]
    public void TC7_Allergy_DrugNameMatch_RaisesCriticalAlert()
    {
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string> { "Alphacillin" }, new List<string>(), new List<string>());
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 300, 1, "Oral", "7 days");

        var alerts = new AllergyRule().Evaluate(rx, patient, BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Critical, alerts[0].Severity);
    }

    [TestMethod]
    public void TC8_Allergy_ClassLevelMatch_RaisesCriticalAlert()
    {
        // Patient allergic to the Penicilin class, not the specific drug name
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string>(), new List<string> { "Penicilin" }, new List<string>());
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 300, 1, "Oral", "7 days");

        var alerts = new AllergyRule().Evaluate(rx, patient, BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Critical, alerts[0].Severity);
    }

    // ---------- FR5 — interaction, including bidirectional symmetry ----------
    [TestMethod]
    public void TC9_Interaction_ActiveOnCardizyme_PrescribeDolorex_RaisesCriticalAlert()
    {
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string>(), new List<string>(), new List<string> { "Cardizyme" });
        var rx = new Prescription(1, "Dolorex", DoseUnit.Mg, 750, 1, "Oral", "7 days");

        var alerts = new InteractionRule().Evaluate(rx, patient, BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Critical, alerts[0].Severity);
    }

    [TestMethod]
    public void TC10_Interaction_ReverseOrder_StillRaisesAlert_ProvesSymmetry()
    {
        // Same table entry, opposite prescribing order — the single most likely bug per the original brief
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string>(), new List<string>(), new List<string> { "Dolorex" });
        var rx = new Prescription(1, "Cardizyme", DoseUnit.Mg, 10, 1, "Oral", "7 days");

        var alerts = new InteractionRule().Evaluate(rx, patient, BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Critical, alerts[0].Severity);
    }

    [TestMethod]
    public void TC11_Interaction_NoTableEntry_RaisesNoAlert()
    {
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string>(), new List<string>(), new List<string> { "Alphacillin" });
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 50, 1, "Oral", "7 days");

        var alerts = new InteractionRule().Evaluate(rx, patient, BuildFormulary()).ToList();

        Assert.AreEqual(0, alerts.Count);
    }

    // ---------- FR6 — duplicate therapy ----------
    [TestMethod]
    public void TC12_Duplicate_SameClassDifferentDrug_RaisesLowAlert()
    {
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string>(), new List<string>(), new List<string> { "Betanol" });
        var rx = new Prescription(1, "Cardizyme", DoseUnit.Mg, 10, 1, "Oral", "7 days");

        var alerts = new DuplicateRule().Evaluate(rx, patient, BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Low, alerts[0].Severity);
    }

    [TestMethod]
    public void TC13_Duplicate_ExactSameDrug_RaisesMediumAlert()
    {
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string>(), new List<string>(), new List<string> { "Betanol" });
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 50, 1, "Oral", "7 days");

        var alerts = new DuplicateRule().Evaluate(rx, patient, BuildFormulary()).ToList();

        Assert.AreEqual(1, alerts.Count);
        Assert.AreEqual(Severity.Medium, alerts[0].Severity);
    }

    // ---------- FR7/FR8 — multi-rule outcome via SafetyChecker ----------
    [TestMethod]
    public void TC14_MultiRule_AllergyAndOverdose_BothAlertsReturned_OutcomeCritical()
    {
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string> { "Alphacillin" }, new List<string>(), new List<string>());
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 600, 1, "Oral", "7 days"); // over max (500) + allergy

        var result = BuildChecker().RunChecks(rx, patient, BuildFormulary());

        Assert.AreEqual(2, result.Alerts.Count);
        Assert.AreEqual(Severity.Critical, result.Outcome);
    }

    [TestMethod]
    public void TC15_MultiRule_ThreeRulesFire_AllAlertsReturned_DoesNotStopAtFirst()
    {
        var patient = new PatientContext(1, "Test", "Patient",
            new List<string>(), new List<string>(), new List<string> { "Betanol", "Cardizyme" });
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 5, 1, "Oral", "7 days");
        // Fires: DoseRange (under min), Duplicate (exact same drug already active)

        var result = BuildChecker().RunChecks(rx, patient, BuildFormulary());

        Assert.IsTrue(result.Alerts.Count >= 2);
    }

    [TestMethod]
    public void TC16_CleanPrescription_ReturnsZeroAlerts_OutcomeNone()
    {
        var rx = new Prescription(1, "Dolorex", DoseUnit.Mg, 750, 1, "Oral", "7 days");
        var result = BuildChecker().RunChecks(rx, CleanPatient(), BuildFormulary());

        Assert.AreEqual(Severity.None, result.Outcome);
        Assert.AreEqual(0, result.Alerts.Count);
    }
}