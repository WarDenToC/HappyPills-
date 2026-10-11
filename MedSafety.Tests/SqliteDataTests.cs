using MedSafety.Core;
using MedSafety.Core.Audit;
using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Rules;
using MedSafety.Core.Security;
using MedSafety.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MedSafety.Tests;

// Integration tests: the data layer against a real SQLite database held in memory
[TestClass]
public class SqliteDataTests
{
    private SqliteConnection _connection = null!;
    private IDbContextFactory<MedSafetyDbContext> _factory = null!;

    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();   // the database exists only while this stays open
        _factory = new TestDbFactory(_connection);

        using var db = _factory.CreateDbContext();
        DatabaseSeeder.EnsureSeeded(db);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _connection.Dispose();
    }

    [TestMethod]
    public void LoadFormulary_SeededDatabase_Returns4DrugsAnd1Interaction()
    {
        var formulary = new SqliteReferenceData(_factory).LoadFormulary();

        Assert.AreEqual(4, formulary.Entries.Count);
        Assert.AreEqual(1, formulary.Interactions.Count);
    }

    [TestMethod]
    public void EnsureSeeded_CalledTwice_DoesNotDuplicateDrugs()
    {
        using (var db = _factory.CreateDbContext())
        {
            DatabaseSeeder.EnsureSeeded(db);
        }

        var formulary = new SqliteReferenceData(_factory).LoadFormulary();

        Assert.AreEqual(4, formulary.Entries.Count);
    }

    [TestMethod]
    public void LoadPatients_SeededDatabase_PatientListsSurviveRoundTrip()
    {
        var patients = new SqliteReferenceData(_factory).LoadPatients();

        var kai = patients.First(p => p.PatientId == 1);
        var riya = patients.First(p => p.PatientId == 2);

        CollectionAssert.Contains(kai.DrugAllergies, "alphacillin");
        CollectionAssert.Contains(riya.ActiveMeds, "Cardizyme");
    }

    [TestMethod]
    public void LoadedFromDatabase_RiyaPrescribedDolorex_OutcomeIsCritical()
    {
        var data = new SqliteReferenceData(_factory);
        var formulary = data.LoadFormulary();
        var riya = data.LoadPatients().First(p => p.PatientId == 2);
        var rx = new Prescription(2, "Dolorex", DoseUnit.Mg, 750, 1, "Oral", "7 days");

        var checker = new SafetyChecker(new IPrescriptionRule[]
        {
            new DoseRangeRule(), new AllergyRule(), new InteractionRule(), new DuplicateRule()
        });
        var result = checker.RunChecks(rx, riya, formulary);

        Assert.AreEqual(Severity.Critical, result.Outcome);
    }

    [TestMethod]
    public void SqliteAuditLog_WriteThenReadFromNewInstance_EntryIsPersisted()
    {
        new SqliteAuditLog(_factory).Write(
            new AuditEntry(DateTime.UtcNow, "Alice", Role.Prescriber, "CreatePrescription", "Detail"));

        var entries = new SqliteAuditLog(_factory).GetAll();

        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual("Alice", entries[0].Actor);
        Assert.AreEqual(Role.Prescriber, entries[0].Role);
        Assert.AreEqual("CreatePrescription", entries[0].Action);
    }

    [TestMethod]
    public void Authorize_WithSqliteAuditLog_DenialIsStoredInDatabase()
    {
        var log = new SqliteAuditLog(_factory);
        var service = new AuthorizationService(log);

        var allowed = service.Authorize("Bob", Role.Prescriber, Permission.ReviewPrescription);

        Assert.IsFalse(allowed);
        var entries = log.GetAll();
        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual("AccessDenied", entries[0].Action);
    }

    // Builds every context from the same open in-memory connection
    private class TestDbFactory : IDbContextFactory<MedSafetyDbContext>
    {
        private readonly SqliteConnection _connection;

        public TestDbFactory(SqliteConnection connection)
        {
            _connection = connection;
        }

        public MedSafetyDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<MedSafetyDbContext>()
                .UseSqlite(_connection)
                .Options;
            return new MedSafetyDbContext(options);
        }
    }
}