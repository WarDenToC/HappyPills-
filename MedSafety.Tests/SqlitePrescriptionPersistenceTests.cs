using MedSafety.Core;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Data;
using MedSafety.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MedSafety.Tests;

// Integration tests: PrescriptionStore + SQLite. A second store over the same database stands in for a restart.
[TestClass]
public class SqlitePrescriptionPersistenceTests
{
    private SqliteConnection _connection = null!;
    private IDbContextFactory<MedSafetyDbContext> _factory = null!;

    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _factory = new TestDbFactory(_connection);

        using var db = _factory.CreateDbContext();
        DatabaseSeeder.EnsureSeeded(db);
    }

    [TestCleanup]
    public void Cleanup() => _connection.Dispose();

    private PrescriptionStore NewStore() => new(new SqlitePrescriptionPersistence(_factory));

    private PatientContext Kai() =>
        new SqliteReferenceData(_factory).LoadPatients().First(p => p.PatientId == 1);

    private static SafetyCheckResult CriticalResult() =>
        new(new List<Alert> { new(Severity.Critical, RuleType.Allergies, "Patient has an allergy to Alphacillin") },
            Severity.Critical);

    [TestMethod]
    public void SubmittedPrescription_SurvivesRestart_WithAlertsAndPatient()
    {
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 300, 1, "Oral", "7 days");
        NewStore().Submit(rx, Kai(), CriticalResult());

        var reloaded = NewStore().All;

        Assert.AreEqual(1, reloaded.Count);
        Assert.AreEqual("Alphacillin", reloaded[0].Prescription.DrugName);
        Assert.AreEqual(300m, reloaded[0].Prescription.DoseAmount);
        Assert.AreEqual("Kai", reloaded[0].Patient.Firstname);
        Assert.AreEqual(Severity.Critical, reloaded[0].SafetyResult.Outcome);
        Assert.AreEqual(1, reloaded[0].SafetyResult.Alerts.Count);
        Assert.AreEqual("Patient has an allergy to Alphacillin", reloaded[0].SafetyResult.Alerts[0].Message);
        Assert.AreEqual("Submitted", reloaded[0].Status);
    }

    [TestMethod]
    public void ReviewDecision_SurvivesRestart()
    {
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 300, 1, "Oral", "7 days");
        var store = NewStore();
        var record = store.Submit(rx, Kai(), CriticalResult());
        store.Decide(record.Id, "Rejected", "Patient is allergic");

        var reloaded = NewStore().All.Single();

        Assert.AreEqual("Rejected", reloaded.Status);
        Assert.AreEqual("Patient is allergic", reloaded.RejectionReason);
        Assert.IsNotNull(reloaded.ReviewedAt);
    }

    [TestMethod]
    public void IdsKeepIncreasing_AfterRestart()
    {
        var rx = new Prescription(1, "Alphacillin", DoseUnit.Mg, 300, 1, "Oral", "7 days");
        var first = NewStore().Submit(rx, Kai(), CriticalResult());

        var second = NewStore().Submit(rx, Kai(), CriticalResult());

        Assert.AreEqual(first.Id + 1, second.Id);
    }

    private class TestDbFactory : IDbContextFactory<MedSafetyDbContext>
    {
        private readonly SqliteConnection _connection;

        public TestDbFactory(SqliteConnection connection) => _connection = connection;

        public MedSafetyDbContext CreateDbContext() =>
            new(new DbContextOptionsBuilder<MedSafetyDbContext>().UseSqlite(_connection).Options);
    }
}