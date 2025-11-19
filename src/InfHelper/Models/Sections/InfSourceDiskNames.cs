using InfHelper.Exceptions;
using System.Collections.Generic;

namespace InfHelper.Models.Sections;

public sealed record InfSourceDiskNames(List<Key> Keys) : BaseSection(Keys)
{
	public static InfSourceDiskNames Parse(InfData data)
	{
		var sourceDiskNamesCategory = data.Categories
			.Find(c => c.IsNamed("SourceDisksNames"))
			?? throw new RequiredSectionMissingException("SourceDisksNames"); ;

		return new InfSourceDiskNames(sourceDiskNamesCategory.Keys);
	}
}
