using InfHelper.Exceptions;
using System.Collections.Generic;

namespace InfHelper.Models.Sections;

public sealed record InfSourceDiskFiles(List<Key> Keys) : BaseSection(Keys)
{
	public static InfSourceDiskFiles Parse(InfData data)
	{
		var sourceDiskFilesCategory = data.Categories
			.Find(c => c.IsNamed("SourceDisksFiles"))
			?? throw new RequiredSectionMissingException("SourceDisksFiles");

		return new InfSourceDiskFiles(sourceDiskFilesCategory.Keys);
	}
}
