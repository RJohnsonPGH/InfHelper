using InfHelper.Exceptions;
using InfHelper.Models.Sections;
using InfHelper.Models.Sections.Entries;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models;

/// <summary>
/// Represents a strongly typed view of parsed INF file data, providing access to key sections such as version
/// information, manufacturers, strings, and section keys.
/// </summary>
/// <remarks>This record is must be constructed by using the <see cref="Parse"/> method. It
/// exposes commonly used sections in a type-safe manner, allowing consumers to access and process INF file contents
/// without manual parsing.</remarks>
public sealed record TypedInfData
{
	private TypedInfData(
		IEnumerable<InfEntry> version, 
		InfSection<InfManufacturerEntry> manufacturers,
		IEnumerable<InfEntry> sourceDiskNames,
		IEnumerable<InfEntry> sourceDiskFiles,
		IEnumerable<InfEntry> strings,
		IEnumerable<Section> allSections)
	{
		Version = version;
		Manufacturers = manufacturers;
		SourceDiskNames = sourceDiskNames;
		SourceDiskFiles = sourceDiskFiles;
		Strings = strings;
		AllSections = allSections;
	}

	public IEnumerable<InfEntry> Version { get; init; }
	public InfSection<InfManufacturerEntry> Manufacturers { get; init; }
	public IEnumerable<InfEntry> SourceDiskNames { get; init; }
	public IEnumerable<InfEntry> SourceDiskFiles { get; init; }
	public IEnumerable<InfEntry> Strings { get; init; }
	public IEnumerable<Section> AllSections { get; init; }

	public static TypedInfData Parse(InfData data)
	{
		// Merge duplicate data sections before parsing, and convert to a dictionary for faster lookups
		var sections = CreateSectionCollection(data);

		var version = GetSection<InfEntry>(sections, "Version");
		var manufacturers = GetSection<InfManufacturerEntry>(sections, "Manufacturer");
		var sourceDiskNames = GetRootSection<InfEntry>(sections, "SourceDisksNames")
			.SelectMany(x => x.Values);
		var sourceDiskFiles = GetRootSection<InfEntry>(sections, "SourceDisksFiles")
			.SelectMany(x => x.Values);
		var strings = GetRootSection<InfEntry>(sections, "Strings")
			.SelectMany(x => x.Values);

		return new(null!, manufacturers, sourceDiskNames, sourceDiskFiles, strings, data.Sections);
	}

	/// <summary>
	/// Creates a nested dictionary structure that organizes entries from the specified INF data by section name, section
	/// extension, and entry name.
	/// </summary>
	/// <remarks>Section names are split at the first period ('.') character, with the part before the period used
	/// as the section name and the part after as the section extension. Entry names are compared using case-insensitive
	/// string comparison. If multiple entries share the same name within a section extension, the last entry encountered
	/// will overwrite previous ones.</remarks>
	/// <param name="data">The INF data containing sections and entries to be organized into the dictionary structure.</param>
	/// <returns>A dictionary where each key is a section name, each value is a dictionary of section extensions, and each section
	/// extension maps to a dictionary of entry names and their corresponding entries.</returns>
	private static InfSectionCollection CreateSectionCollection(InfData data)
	{
		var sections = new InfSectionCollection();
		foreach (var section in data.Sections)
		{
			// Split the section name
			int index = section.Name.IndexOf('.');
			var sectionName = index < 0 ? section.Name : section.Name[..index]; // If there is no '.', use the entire section name
			var sectionNameExtension = index < 0 ? string.Empty : section.Name[(index + 1)..]; // If there is no '.', use an empty string as the extension

			// The section does not yet exist in the dictionary, so add it
			if (!sections.TryGetValue(sectionName, out var sectionExtensions))
			{
				sectionExtensions = [];
				sections[sectionName] = sectionExtensions;
			}

			// The section extension does not yet exist in the dictionary, so add it
			if (!sectionExtensions.TryGetValue(sectionNameExtension, out var entries))
			{
				entries = [];
				sectionExtensions[sectionNameExtension] = entries;
			}

			// Add all entries from the current section to the merged section, overwriting any duplicates
			foreach (var entry in section)
			{
				entries[entry.Name] = entry;
			}
		}
		return sections;
	}

	/// <summary>
	/// Generates a sequence of extended section names based on the provided entry values.
	/// </summary>
	/// <remarks>The first entry value is used as the base section name. Each subsequent section name is constructed
	/// by concatenating the base section name with the value of each additional entry, separated by a period.</remarks>
	/// <param name="entryValues">A collection of <see cref="EntryValue"/> objects representing the base and additional values used to construct
	/// section names.</param>
	/// <returns>An enumerable collection of strings containing the base section name followed by extended section names formed by
	/// combining the base with each additional entry value.</returns>
	internal static IEnumerable<string> GetExtendedSections(IEnumerable<EntryValue> entryValues)
	{
		// The first entry value is the base section name
		var sectionName = entryValues.First();

		// Example input of a Manufacturer: 'DriverName,NTAMD64'
		// The valid section names from this are ['DriverName', 'DriverName.NTAMD64']
		// So we yield the base section name first
		yield return sectionName.Value;

		// Then yield each extended section name
		foreach (var extendedSectionSuffix in entryValues.Skip(1))
		{
			yield return $"{sectionName.Value}.{extendedSectionSuffix.Value}";
		}
	}

	/// <summary>
	/// Retrieves a strongly typed section from the specified collection by name. Throws an exception if the section or its
	/// extension is not found.
	/// </summary>
	/// <typeparam name="T">The type of section to retrieve. Must implement <see cref="IInfSection{T}"/>.</typeparam>
	/// <param name="allSections">The collection containing all available sections from which to retrieve the specified section.</param>
	/// <param name="sectionName">The name of the section to retrieve. May include an extension separated by a period ('.').</param>
	/// <returns>An <see cref="InfSection{T}"/> representing the requested section and its extension.</returns>
	/// <exception cref="RequiredSectionMissingException">Thrown if the specified section or its extension does not exist in the collection.</exception>
	internal static InfSection<T> GetSection<T>(InfSectionCollection allSections, string sectionName) where T : IInfSection<T>
	{
		// Split the section name
		int index = sectionName.IndexOf('.');

		// Create an array with the section name and extension
		string[] splitSectionName = [
			index < 0 ? sectionName : sectionName[..index], // If there is no '.', use the entire section name
			index < 0 ? string.Empty : sectionName[(index + 1)..] // If there is no '.', use an empty string as the extension
		];

		// Try to get the section and extension from the collection
		if (!allSections.TryGetValue(splitSectionName[0], out var section) ||
			!section.TryGetValue(splitSectionName[1], out var extSection))
		{
			// The section or extension does not exist, so throw an exception
			throw new RequiredSectionMissingException(splitSectionName[0], splitSectionName[1]);
		}

		return new(sectionName, allSections, extSection);
	}

	/// <summary>
	/// Retrieves all sections with the specified name from the provided collection.
	/// </summary>
	/// <typeparam name="T">The type of section to retrieve. Must implement the IInfSection<T> interface.</typeparam>
	/// <param name="allSections">The collection containing all available sections.</param>
	/// <param name="sectionName">The name of the root section to retrieve. Cannot be null or empty.</param>
	/// <returns>An enumerable collection of root sections of type InfSection<T> that match the specified name.</returns>
	/// <exception cref="RequiredSectionMissingException">Thrown if a section with the specified name does not exist in the collection.</exception>
	internal static IEnumerable<InfSection<T>> GetRootSection<T>(InfSectionCollection allSections, string sectionName) where T : IInfSection<T>
	{
		// Try to get the section and extension from the collection
		if (!allSections.TryGetValue(sectionName, out var section))
		{
			// The section or extension does not exist, so throw an exception
			throw new RequiredSectionMissingException(sectionName, string.Empty);
		}

		return section
			.Select(extSection => new InfSection<T>(extSection.Key, allSections, extSection.Value));
	}
}
