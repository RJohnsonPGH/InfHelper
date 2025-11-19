using System;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections;

public sealed record InfModel(string Name, IEnumerable<Key> Keys)
{

	//var decoratedSections = data.Categories.Where(x => decoratedSectionNames.Contains(x.Name));
	//

	//	decoratedSections.Select(y => {
	//						// Each Model KeyValue[0] is the DDInstall section name
	//						var installSectionName = y.Keys.Select(z => z.KeyValues[0]);
	//						return new Model(y.Name, []);
	//})
	public static IEnumerable<InfModel> Parse(Key key, InfData data)
	{
		// Combine the manufacturer key values to get the decorated section names
		// e.g. "Standard,NTAMD64" becomes ["Standard", "Standard.NTAMD64"]
		var decoratedSectionNames = InfUtil.GeneratedDectoratedSections(key.KeyValues);

		foreach (var sectionName in decoratedSectionNames)
		{
			var decoratedSection = data.Categories.SingleOrDefault(x => x.IsNamed(sectionName))
				?? throw new InvalidOperationException($"Model does not exist: {sectionName}");
			yield return new(decoratedSection.Name, decoratedSection.Keys);
		}
	}
}
