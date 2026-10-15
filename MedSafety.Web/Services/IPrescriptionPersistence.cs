namespace MedSafety.Web.Services;

public interface IPrescriptionPersistence
{
    IReadOnlyList<PrescriptionRecord> LoadAll();
    int Insert(PrescriptionRecord record);   // returns the id the database assigned
    void Update(PrescriptionRecord record);  // saves the review outcome
}