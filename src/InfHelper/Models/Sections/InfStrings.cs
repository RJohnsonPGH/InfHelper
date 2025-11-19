using InfHelper.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections;

//https://learn.microsoft.com/en-us/windows-hardware/drivers/install/inf-strings-section

//Required
public sealed record InfStrings(List<Key> Keys, string? Localization = null)
{
	public static IEnumerable<InfStrings> Parse(InfData data)
	{
		var stringsCategory = data.Categories
			.Where(x => x.IsBaseCategory("Strings"))
			?? throw new RequiredSectionMissingException("Strings");

		var defaultStrings = stringsCategory.SingleOrDefault(x => x.IsNamed("Strings"));

		if (defaultStrings is not null)
		{
			yield return new InfStrings(defaultStrings.Keys);
		}

		var localizedStrings = stringsCategory
			.Except(defaultStrings is not null ? [defaultStrings] : []);

		foreach (var currentLocalizedStrings in localizedStrings)
		{
			yield return new InfStrings(currentLocalizedStrings.Keys, currentLocalizedStrings.Name);
		}
	}
}
