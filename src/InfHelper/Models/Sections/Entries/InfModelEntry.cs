using InfHelper.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections.Entries;

/// <summary>
/// Represents a device model entry in an INF file, providing access to its installation section and associated device
/// identifiers.
/// </summary>
public sealed record InfModelEntry : InfEntry, IInfSection<InfModelEntry>
{
	/// <summary>
	/// Initializes a new instance of the InfModel class using the specified section name, entry, and collection of all
	/// sections.
	/// </summary>
	/// <remarks>This constructor sets up the InfModel with references to installation data and device identifiers
	/// based on the provided entry and section collection. The entry's values are used to determine installation
	/// information and device IDs.</remarks>
	/// <param name="sectionName">The name of the section to associate with this model. This value identifies the section within the INF file.</param>
	/// <param name="entry">The entry containing the name and values for the section. Must not be null.</param>
	/// <param name="allSections">A collection of all sections available in the INF file. Used to resolve related section data.</param>
	private InfModelEntry(string sectionName, Entry entry, InfSectionCollection allSections) : base(sectionName, entry.Name, entry.Values)
	{
		_install = new(() => 
			TypedInfData.GetSection<InfInstallEntry>(allSections, entry.Values[0].Value));
		DeviceIds = entry
			.Values[1..]
			.Select(x => x.Value);
	}

	/// <summary>
	/// Gets the installation information associated with the current context.
	/// </summary>
	// Each model must have exactly one install section - this should never fail
	// Validation to ensure that at least one section eixsts occurs in the GetSection method and duplicate
	// Section names are merged prior to this point
	public InfInstallEntry Install => _install.Value.Single(); 
	private readonly Lazy<InfSection<InfInstallEntry>> _install;
	public IEnumerable<string> DeviceIds { get; }

	/// <inheritdoc/>
	static InfModelEntry IInfSection<InfModelEntry>.Create(string sectionName, Entry entry, InfSectionCollection allSections)
	{
		// Models must have at least two entries: the install section, and the model ID
		if (entry.Values.Count < 2)
		{
			throw new RequiredEntryMalformedException(entry.Name, sectionName, entry);
		}
		return new(sectionName, entry, allSections);
	}
}