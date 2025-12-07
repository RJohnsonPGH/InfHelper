using InfHelper.Exceptions;
using InfHelper.Models.Sections.Entries;
using System;
using System.Collections.Generic;

namespace InfHelper.Models.Sections;

public sealed record InfVersion
{
	private InfVersion(string signature, string classType, Guid classGuid, string provider, InfDriverVer driverVer)
	{
		Signature = signature;
		Class = classType;
		ClassGuid = classGuid;
		Provider = provider;
		DriverVer = driverVer;
	}

	public string Signature { get; }
	public string Class { get; }
	public Guid ClassGuid { get; }
	public string Provider { get; }
	public InfDriverVer DriverVer { get; }

	//public static InfVersion Parse(IEnumerable<InfVersionEntry> entries)
	//{
	//	var entryDictionary = entries.ToDictionary(
	//		entry => entry.Name, 
	//		entry => entry,
	//		StringComparer.OrdinalIgnoreCase);

	//	var signature = GetRequiredEntry(entryDictionary, "Signature").Values;
	//	var classType = GetRequiredEntry(entryDictionary, "Class");
	//	var guid = GetRequiredEntry(entryDictionary, "ClassGUID");
	//	var provider = GetRequiredEntry(entryDictionary, "Provider");
	//	var driverVer = InfDriverVer.Parse(GetRequiredEntry(entryDictionary, "DriverVer"));
	//	if (!Guid.TryParse(guid, out var classGuid))
	//	{
	//		throw new RequiredEntryMalformedException("Version", "ClassGUID", guid);
	//	}

	//	// Implicit string operator on the Entry type which calls entry.Values.GetUnparsedValue();
	//	return new(signature, classType, classGuid, provider, driverVer);
	//}

	private static InfVersionEntry GetRequiredEntry(Dictionary<string, InfVersionEntry> dictionary, string key)
	{
		if (!dictionary.TryGetValue(key, out var entry))
		{
			throw new RequiredEntryMissingException("Version", key);
		}
		return entry;
	}
}

public sealed record InfVersionEntry : InfEntry, IInfSection<InfVersionEntry>
{
	internal InfVersionEntry(string name, Entry entry) : base(name, entry.Name, entry.Values) { }

	static InfVersionEntry IInfSection<InfVersionEntry>.Create(string sectionName, Entry entry, InfSectionCollection _) =>
		new(sectionName, entry);
}

/// <summary>
/// Represents the version information specified by a DriverVer entry in an INF file, including the driver date and
/// version number.
/// </summary>
/// <remarks>This record provides strongly typed access to the date and version components of a DriverVer entry.
/// Instances of this type are created by parsing an INF entry using the Parse method.</remarks>
public sealed record InfDriverVer
{
	private InfDriverVer(DateOnly date, Version version)
	{
		Date = date;
		Version = version;
	}

	public DateOnly Date { get; }
	public Version Version { get; }

	public static InfDriverVer Parse(Entry driverVer)
	{
		if (driverVer.Values.Count != 2 ||
			!DateOnly.TryParse(driverVer.Values[0].Value, out var date) ||
			!Version.TryParse(driverVer.Values[1].Value, out var version))
		{
			throw new RequiredEntryMalformedException("Version", "DriverVer", driverVer.Values.GetPrimitiveValue());
		}
		return new InfDriverVer(date, version);
	}
}
