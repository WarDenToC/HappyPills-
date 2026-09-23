using System.ComponentModel;
using MedSafety.Core.Enum;

namespace MedSafety.Core.Models.FormularyTable;

public class Formulary
{
    public IReadOnlyList<DrugEntry> Entries { get; }
    public IReadOnlyList<InteractionEntry> Interactions { get; }

    public Formulary(IReadOnlyList<DrugEntry> entries, IReadOnlyList<InteractionEntry> interactions)
    {
        Entries = entries;
        Interactions = interactions;
    }

    //FindEntry: used to find if a prescribed drug is within the list of drug within data.
    public DrugEntry? FindEntry(string drugName)
    {
        foreach (DrugEntry entry in Entries)
        {
            if (entry.DrugName.ToLower() == drugName.ToLower())
                return entry;
        }
        return null;
    }

    //Used to check drugA to drugB or vice versa, if two drugs have clashes
    public InteractionEntry? FindInteraction(string a, string b)
    {
        foreach (InteractionEntry interaction in Interactions)
        {
            if ((interaction.DrugA.ToLower() == a.ToLower() && interaction.DrugB.ToLower() == b.ToLower()) ||
                (interaction.DrugA.ToLower() == b.ToLower() && interaction.DrugB.ToLower() == a.ToLower()))
            {
                return interaction;
            }
        }
        return null;
    }
}