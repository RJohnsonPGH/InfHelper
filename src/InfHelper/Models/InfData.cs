using System;
using System.Collections.Generic;
using System.Linq;
using InfHelper.Exceptions;

namespace InfHelper.Models;

public sealed class InfData
{
	public List<Section> Sections { get; set; } = [];

	public Section this[string name]
	{
		get
		{
			var results = Sections.Where(k => string.Compare(k.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
			if (results.Count() > 1)
			{
				throw new MultipleCategoriesWithSameNameException();
			}
			return results.FirstOrDefault() ??
				throw new KeyNotFoundException();
		}
		set
		{
			int index = -1;
			for (var i = 0; i < Sections.Count; i++)
			{
				if (Sections[i].Name != name) continue;
				//id match found
				if (index == -1)
				{
					index = i;
				}
				else
				{
					//multiple id match
					throw new MultipleCategoriesWithSameNameException();
				}
			}

			//add new
			if (index == -1)
			{
				Sections.Add(value);
			}

			//udpate existing
			else
			{
				Sections[index] = value;
			}
		}
	}

	public Entry FindEntryById(string keyId)
	{
		return Sections
			.SelectMany(s => s.Entries)
			.LastOrDefault(e => string.Equals(e.Name, keyId, StringComparison.OrdinalIgnoreCase)) ?? // Last entry wins
			throw new InvalidOperationException("Entry not found.");
	}
}