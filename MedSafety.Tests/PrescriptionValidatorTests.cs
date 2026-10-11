using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MedSafety.Tests;

[TestClass]
public class PrescriptionValidatorTests
{
    private static Formulary BuildFormulary() => new(
        new List<DrugEntry> { new(1, "Betanol", DrugClass.BetaBlocker, DoseUnit.Mg, 25, 100) },
        new List<InteractionEntry>());

    [TestMethod]
    public void Validate_EmptyDrugName_ReturnsOneError()
    {
        var rx = new Prescription(1, "", DoseUnit.Mg, 50, 1, "Oral", "7 days");
        var errors = PrescriptionValidator.Validate(rx, BuildFormulary());
        Assert.AreEqual(1, errors.Count);
    }

    [TestMethod]
    public void Validate_DoseZero_ReturnsError()
    {
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 0, 1, "Oral", "7 days");
        var errors = PrescriptionValidator.Validate(rx, BuildFormulary());
        Assert.IsTrue(errors.Count >= 1);
    }

    [TestMethod]
    public void Validate_DoseNegative_ReturnsError()
    {
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, -5, 1, "Oral", "7 days");
        var errors = PrescriptionValidator.Validate(rx, BuildFormulary());
        Assert.IsTrue(errors.Count >= 1);
    }

    [TestMethod]
    public void Validate_EmptyRoute_ReturnsError()
    {
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 50, 1, "", "7 days");
        var errors = PrescriptionValidator.Validate(rx, BuildFormulary());
        Assert.IsTrue(errors.Count >= 1);
    }

    [TestMethod]
    public void Validate_UnknownDrug_ReturnsOneErrorContainingDrugName()
    {
        var rx = new Prescription(1, "PTY1", DoseUnit.Mg, 50, 1, "Oral", "7 days");
        var errors = PrescriptionValidator.Validate(rx, BuildFormulary());

        Assert.AreEqual(1, errors.Count);
        StringAssert.Contains(errors[0], "PTY1");
    }

    [TestMethod]
    public void Validate_FullyValidPrescription_ReturnsZeroErrors()
    {
        var rx = new Prescription(1, "Betanol", DoseUnit.Mg, 50, 1, "Oral", "7 days");
        var errors = PrescriptionValidator.Validate(rx, BuildFormulary());
        Assert.AreEqual(0, errors.Count);
    }
}