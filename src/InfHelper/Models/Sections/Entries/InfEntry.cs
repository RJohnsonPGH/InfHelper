using System.Collections.Generic;

namespace InfHelper.Models.Sections.Entries;

/// <summary>
/// Represents an entry within a section of an INF file, containing a name and a collection of associated values.
/// </summary>
public record InfEntry : IInfSection<InfEntry>
{
	/// <summary>
	/// Initializes a new instance of the InfEntry class with the specified section name, entry name, and associated
	/// values.
	/// </summary>
	/// <param name="sectionName">The name of the section to which this entry belongs. Cannot be null.</param>
	/// <param name="name">The name of the entry within the section. Cannot be null.</param>
	/// <param name="values">A list of values associated with the entry. Cannot be null.</param>
	internal InfEntry(string sectionName, string name, List<EntryValue> values)
	{
		SectionName = sectionName;
		Name = name;
		Values = values;
	}

	/// <summary>
	/// Gets the name of the section associated with this entry.
	/// </summary>
	public string SectionName { get; }
	/// <summary>
	/// Gets the name associated with the current entry.
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// Gets a read-only list of values associated with this entry.
	/// </summary>
	public IReadOnlyList<EntryValue> Values { get; }

	static InfEntry IInfSection<InfEntry>.Create(string sectionName, Entry entry, InfSectionCollection _) =>
		new(sectionName, entry.Name, entry.Values);
}
