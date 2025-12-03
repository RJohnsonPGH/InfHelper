using InfHelper.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

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

	public static InfVersion Parse(IEnumerable<InfVersionEntry> entries)
	{
		var entryDictionary = entries.ToDictionary(
			entry => entry.Name, 
			entry => entry.Entry,
			StringComparer.OrdinalIgnoreCase);

		var signature = GetRequiredEntry(entryDictionary, "Signature");
		var classType = GetRequiredEntry(entryDictionary, "Class");
		var guid = GetRequiredEntry(entryDictionary, "ClassGUID");
		var provider = GetRequiredEntry(entryDictionary, "Provider");
		var driverVer = InfDriverVer.Parse(GetRequiredEntry(entryDictionary, "DriverVer"));
		if (!Guid.TryParse(guid, out var classGuid))
		{
			throw new RequiredEntryMalformedException("Version", "ClassGUID", guid);
		}

		// Implicit string operator on the Entry type which calls entry.Values.GetUnparsedValue();
		return new(signature, classType, classGuid, provider, driverVer);
	}

	private static Entry GetRequiredEntry(Dictionary<string, Entry> dictionary, string key)
	{
		if (!dictionary.TryGetValue(key, out var entry))
		{
			throw new RequiredEntryMissingException("Version", key);
		}
		return entry;
	}
}

/// <summary>
/// Represents a version entry in an INF file, providing access to version-specific metadata and values.
/// </summary>
/// <remarks>
/// The Version section is unique. There are no extended Version sections (e.g. Version.XXXX), so it is a root node.
/// However, it does not contain any child sections (as in, Entries in the Version section do not refer to other sections as is the case with Manufacturer).
/// This is the reason for the mixed inheritance of InfLeafEntry and IInfRoot interface.
/// </remarks>
public sealed record InfVersionEntry : InfLeafEntry, IInfRoot<InfVersionEntry>
{
	internal InfVersionEntry(string name, Entry entry) : base(name, string.Empty, entry) { }

	static InfVersionEntry IInfRoot<InfVersionEntry>.Create(string name, Entry entry, InfSectionCollection _) =>
		new(name, entry);
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
