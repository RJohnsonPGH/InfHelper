using InfHelper.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections;

// Required
// https://learn.microsoft.com/en-us/windows-hardware/drivers/install/inf-version-section
public sealed record InfVersion(List<Key> Keys) : BaseSection(Keys)
{
	public static InfVersion Parse(InfData data)
	{
		var keys = data.Categories.SingleOrDefault(c => c.IsNamed("Version"))
			?? throw new RequiredSectionMissingException("Version");

		return new InfVersion(keys.Keys);
	}
}