using InfHelper.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections;

public abstract record BaseSection(List<Key> Keys)
{
	public Key this[string id]
	{
		get
		{
			var results = Keys.Where(k => string.Compare(k.Id, id, StringComparison.OrdinalIgnoreCase) == 0);

			if (results.Count() > 1)
			{
				throw new MultipleKeysWithSameIdException();
			}

			return results.FirstOrDefault() ??
				throw new KeyNotFoundException();
		}
	}
}
