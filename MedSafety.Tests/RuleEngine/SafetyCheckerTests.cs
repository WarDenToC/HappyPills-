using MedSafety.Core;
using MedSafety.Core.Models;
using MedSafety.Core.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MedSafety.Tests.RuleEngine;

[TestClass]
public class SafetyCheckerTests
{
    [TestMethod]
    public void RunChecks_NoRulesFire_ReturnsNoneOutcomeAndNoAlerts()
    {
        var mockRule = new Mock<IPrescriptionRule>();
        mockRule.Setup(r => r.Evaluate(
                It.IsAny<Prescription>(), It.IsAny<PatientContext>(), It.IsAny<Formulary>()))
            .Returns(new List<Alert>());

        var checker = new SafetyChecker(new[] { mockRule.Object });

        var result = checker.RunChecks(new Prescription(), new PatientContext(), new Formulary());

        Assert.AreEqual(Severity.None, result.Outcome);
        Assert.AreEqual(0, result.Alerts.Count);
    }

    [TestMethod]
    public void RunChecks_MultipleRulesFire_OutcomeIsHighestSeverity()
    {
        var lowRule = new Mock<IPrescriptionRule>();
        lowRule.Setup(r => r.Evaluate(
                It.IsAny<Prescription>(), It.IsAny<PatientContext>(), It.IsAny<Formulary>()))
            .Returns(new List<Alert> { new("LowRule", Severity.Low, "n/a", "low severity issue") });

        var criticalRule = new Mock<IPrescriptionRule>();
        criticalRule.Setup(r => r.Evaluate(
                It.IsAny<Prescription>(), It.IsAny<PatientContext>(), It.IsAny<Formulary>()))
            .Returns(new List<Alert> { new("CriticalRule", Severity.Critical, "n/a", "critical severity issue") });

        var checker = new SafetyChecker(new[] { lowRule.Object, criticalRule.Object });

        var result = checker.RunChecks(new Prescription(), new PatientContext(), new Formulary());

        Assert.AreEqual(Severity.Critical, result.Outcome);
        Assert.AreEqual(2, result.Alerts.Count);
    }

    [TestMethod]
    public void RunChecks_AllFiredRulesReturned_DoesNotStopAtFirstMatch()
    {
        // FR7: all alerts must be returned, not just the first or the highest-severity one
        var ruleA = new Mock<IPrescriptionRule>();
        ruleA.Setup(r => r.Evaluate(
                It.IsAny<Prescription>(), It.IsAny<PatientContext>(), It.IsAny<Formulary>()))
            .Returns(new List<Alert> { new("RuleA", Severity.Critical, "n/a", "reason A") });

        var ruleB = new Mock<IPrescriptionRule>();
        ruleB.Setup(r => r.Evaluate(
                It.IsAny<Prescription>(), It.IsAny<PatientContext>(), It.IsAny<Formulary>()))
            .Returns(new List<Alert> { new("RuleB", Severity.Low, "n/a", "reason B") });

        var checker = new SafetyChecker(new[] { ruleA.Object, ruleB.Object });

        var result = checker.RunChecks(new Prescription(), new PatientContext(), new Formulary());

        Assert.AreEqual(2, result.Alerts.Count);
        Assert.AreEqual(Severity.Critical, result.Outcome);
    }
}