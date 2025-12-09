using System;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections.Entries;

/// <summary>
/// Represents a manufacturer entry in an INF file, providing access to associated models and related section data.
/// </summary>
public sealed record InfManufacturerEntry : InfEntry, IInfSection<InfManufacturerEntry>
{
	/// <summary>
	/// Initializes a new instance of the InfManufacturer class using the specified section name, entry, and
	/// collection of INF sections.
	/// </summary>
	/// <param name="sectionName">The name of the containing section in the INF file. Cannot be null or empty.</param>
	/// <param name="entry">The entry representing the name and values for this manufacturer. Cannot be null.</param>
	/// <param name="allSections">A collection containing all sections from the INF file, used to resolve related model sections. Cannot be null.</param>
	private InfManufacturerEntry(string sectionName, Entry entry, InfSectionCollection allSections) : base(sectionName, entry.Name, entry.Values)
	{
		_models = new(TypedInfData.GetExtendedSections(entry)
			.SelectMany(sectionName =>
				TypedInfData.GetSection<InfModelEntry>(allSections, sectionName)
					.Select(modelSection => modelSection.Value)));
	}

	public IEnumerable<InfModelEntry> Models => _models.Value;
	private readonly Lazy<IEnumerable<InfModelEntry>> _models;

	/// <inheritdoc/>
	static InfManufacturerEntry IInfSection<InfManufacturerEntry>.Create(string sectionName, Entry entry, InfSectionCollection allSections) =>
		new(sectionName, entry, allSections);
}
