using System.Diagnostics;
using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;
using MedSafety.Core.Rules;

namespace MedSafety.Tests;

// NFR2: a full safety check must finish within 500 ms per prescription
[TestClass]
public class PerformanceTests
{
    public TestContext TestContext { get; set; } = null!;

    private const double MaxMsPerCheck = 500;

    private static Formulary BuildFormulary() => new(
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

    // Fixed seed (42): the same prescriptions are generated every run, so results are repeatable
    private static List<(Prescription Rx, PatientContext Patient)> Generate(int count, Formulary formulary)
    {
        var random = new Random(42);
        var drugs = formulary.Entries;
        var cases = new List<(Prescription Rx, PatientContext Patient)>();

        for (int i = 0; i < count; i++)
        {
            var drug = drugs[random.Next(drugs.Count)];
            var activeMed = drugs[random.Next(drugs.Count)].DrugName;

            // One patient in four is allergic to the drug they are prescribed
            var allergies = random.Next(4) == 0 ? new List<string> { drug.DrugName } : new List<string>();
            var patient = new PatientContext(i, "Test", $"Patient{i}",
                allergies, new List<string>(), new List<string> { activeMed });

            // Doses from half the minimum up to 1.5 times the maximum, so some checks raise alerts
            var low = drug.MinDose / 2;
            var high = drug.MaxDose * 1.5m;
            var dose = Math.Round(low + (decimal)random.NextDouble() * (high - low), 1);

            var rx = new Prescription(i, drug.DrugName, DoseUnit.Mg, dose, 1, "Oral", "7 days");
            cases.Add((rx, patient));
        }
        return cases;
    }

    [TestMethod]
    [DataRow(100)]
    public void RunChecks_ManyPrescriptions_EachCheckUnder500Ms(int count)
    {
        var formulary = BuildFormulary();
        var checker = new SafetyChecker(new IPrescriptionRule[]
        {
            new DoseRangeRule(), new AllergyRule(), new InteractionRule(), new DuplicateRule()
        });
        var cases = Generate(count, formulary);

        // Warm-up: the very first call includes one-off start-up cost, so it isn't timed
        checker.RunChecks(cases[0].Rx, cases[0].Patient, formulary);

        var slowest = 0.0;
        var alerts = 0;
        var total = Stopwatch.StartNew();
        foreach (var (rx, patient) in cases)
        {
            var one = Stopwatch.StartNew();
            var result = checker.RunChecks(rx, patient, formulary);
            one.Stop();

            slowest = Math.Max(slowest, one.Elapsed.TotalMilliseconds);
            alerts += result.Alerts.Count;
        }
        total.Stop();

        TestContext.WriteLine($"{count} checks: total {total.Elapsed.TotalMilliseconds:F1} ms, " +
                              $"slowest {slowest:F3} ms, {alerts} alerts raised");

        Assert.IsTrue(slowest < MaxMsPerCheck,
            $"Slowest check took {slowest:F1} ms, limit is {MaxMsPerCheck} ms");
        Assert.IsTrue(alerts > 0,
            "No alerts were raised, so the generated data isn't exercising the rules");
    }
}