namespace InfHelper.Models.Sections.Entries;

/// <summary>
/// Represents an installation entry within an INF file, providing access to the section's name and associated values.
/// </summary>
public sealed record InfInstallEntry : InfEntry, IInfSection<InfInstallEntry>
{
	/// <summary>
	/// Initializes a new instance of the InfInstall class using the specified section name, entry, and section collection.
	/// </summary>
	/// <param name="sectionName">The name of the INF section to associate with this instance. Cannot be null or empty.</param>
	/// <param name="entry">The entry containing the name and values to be used for initialization. Cannot be null.</param>
	/// <param name="_">The collection of INF sections available for reference during initialization.</param>
	private InfInstallEntry(string sectionName, Entry entry, InfSectionCollection _) : base(sectionName, entry.Name, entry.Values) { }

	/// <inheritdoc/>
	static InfInstallEntry IInfSection<InfInstallEntry>.Create(string sectionName, Entry entry, InfSectionCollection allSections) =>
		new(sectionName, entry, allSections);
}