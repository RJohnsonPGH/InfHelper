using InfHelper.Exceptions;
using InfHelper.Models.Sections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models;

public sealed record TypedInfData
{
	public required InfVersion Version { get; init; }
	public required InfRootSection<InfManufacturer> Manufacturers { get; init; }
	//public required InfSourceDiskNames SourceDiskNames { get; init; }
	//public required InfSourceDiskFiles SourceDiskFiles { get; init; }
	public required InfLeafSection<InfStrings> Strings { get; init; }
	public required IEnumerable<Section> Keys { get; init; }

	public static TypedInfData Parse(InfData data)
	{
		// Merge duplicate data sections before parsing, and convert to a dictionary for faster lookups
		var sections = CreateSectionCollection(data);

		// Version is special, so we parse the individual entries using ParseRootSection, then assemble the final object
		var version = InfVersion.Parse(ParseRootSection<InfVersionEntry>(sections, "Version"));
		var manufacturer = ParseRootSection<InfManufacturer>(sections, "Manufacturer");
		//var sourceDiskNames = ParseTopLevelSection<InfSourceDiskNames>(sections);
		//var sourceDiskFiles = ParseTopLevelSection<InfSourceDiskFiles>(sections);
		var strings = ParseLeafSection<InfStrings>(sections, "Strings");
		return new()
		{
			Version = version,
			Manufacturers = manufacturer,
			//SourceDiskNames = sourceDiskNames,
			//SourceDiskFiles = sourceDiskFiles,
			Strings = strings,
			Keys = data.Sections
		};
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
			var sectionName = index < 0 ? section.Name : section.Name[..index];
			var sectionNameExtension = index < 0 ? string.Empty : section.Name[(index + 1)..];

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

	internal static IEnumerable<string> GetExtendedSections(IEnumerable<EntryValue> entryValues)
	{
		if (entryValues.Any(x => x.IsDynamic))
		{
			throw new InvalidOperationException("Cannot generate decorated section names for dynamic key values.");
		}

		var baseSectionName = entryValues.First();
		yield return baseSectionName.Value;

		foreach (var dectoratedSection in entryValues.Skip(1))
		{
			yield return $"{baseSectionName.Value}.{dectoratedSection.Value}";
		}
	}

	internal static InfLeafSection<T> ParseLeafSection<T>(InfSectionCollection allSections, string sectionName) where T : IInfLeaf<T>
	{
		if (!allSections.TryGetValue(sectionName, out var section))
		{
			throw new RequiredSectionMissingException(sectionName);
		}

		return new(sectionName, allSections, section);
	}

	internal static InfBranchSection<T> ParseBranchSection<T>(InfSectionCollection allSections, string sectionName) where T : IInfBranch<T>
	{
		if (!allSections.TryGetValue(sectionName, out var section))
		{
			throw new RequiredSectionMissingException(sectionName);
		}
		return new(sectionName, allSections, section);
	}

	internal static InfRootSection<T> ParseRootSection<T>(InfSectionCollection allSections, string sectionName) where T : IInfRoot<T>
	{
		if (!allSections.TryGetValue(sectionName, out var section) ||
			!section.TryGetValue(string.Empty, out var extSection))
		{
			throw new RequiredSectionMissingException(sectionName);
		}

		return new(sectionName, allSections, extSection);
	}
}
