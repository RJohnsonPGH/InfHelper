using InfHelper.Models.Sections;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models;

public sealed record TypedInfData
{
	public required InfVersion Version { get; init; }
	public required IEnumerable<InfManufacturer> Manufacturers { get; init; }
	public required InfSourceDiskNames SourceDiskNames { get; init; }
	public required InfSourceDiskFiles SourceDiskFiles { get; init; }
	public required IEnumerable<InfStrings> Strings { get; init; }
	public required IEnumerable<Category> Keys { get; init; }

	public static TypedInfData Parse(InfData data)
	{
		var version = InfVersion.Parse(data);
		var manufacturers = InfManufacturer.Parse(data);
		var sourceDiskNames = InfSourceDiskNames.Parse(data);
		var sourceDiskFiles = InfSourceDiskFiles.Parse(data);
		var strings = InfStrings.Parse(data).ToList();

		return new()
		{
			Version = version,
			Manufacturers = manufacturers,
			SourceDiskNames = sourceDiskNames,
			SourceDiskFiles = sourceDiskFiles,
			Strings = strings,
			Keys = data.Categories
		};
	}
}
