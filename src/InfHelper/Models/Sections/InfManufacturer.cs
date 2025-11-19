using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections;

public sealed record InfManufacturer(string Name, IEnumerable<InfModel> Models)
{
	public static IEnumerable<InfManufacturer> Parse(InfData data)
	{
		var manufacturerCategory = data.Categories.FirstOrDefault(c => c.IsNamed("Manufacturer"));

		if (manufacturerCategory is null)
		{
			return [];
		}

		var name = manufacturerCategory.Name;
		return manufacturerCategory
			.Keys
			.Select(x => new InfManufacturer(x.Id, InfModel.Parse(x, data)));
	}
}
