using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;
using Microsoft.EntityFrameworkCore;

namespace MedSafety.Data;

public class SqliteReferenceData
{
    private readonly IDbContextFactory<MedSafetyDbContext> _factory;

    public SqliteReferenceData(IDbContextFactory<MedSafetyDbContext> factory)
    {
        _factory = factory;
    }

    public Formulary LoadFormulary()
    {
        using var db = _factory.CreateDbContext();

        // ToList() first: load the rows, then convert in C# (SQLite can't compare decimals in queries)
        var drugs = db.Drugs.ToList()
            .Select(d => new DrugEntry(d.Id, d.DrugName, d.DrugClass, d.DoseUnit, d.MinDose, d.MaxDose))
            .ToList();
        var interactions = db.Interactions.ToList()
            .Select(i => new InteractionEntry { Id = i.Id, DrugA = i.DrugA, DrugB = i.DrugB, Severity = i.Severity })
            .ToList();

        return new Formulary(drugs, interactions);
    }

    public List<PatientContext> LoadPatients()
    {
        using var db = _factory.CreateDbContext();
        return db.Patients.ToList()
            .Select(p => new PatientContext(p.Id, p.Firstname, p.Lastname, p.DrugAllergies, p.ClassAllergies, p.ActiveMeds))
            .ToList();
    }
}