using System;
using System.Collections.Generic;

namespace InfHelper.Models.Sections;

public class InfSectionCollection : Dictionary<string, InfSectionExtCollection>
{
	public InfSectionCollection() : base(StringComparer.OrdinalIgnoreCase) { }
}

public class InfSectionExtCollection : Dictionary<string, InfSectionEntryCollection>
{
	public InfSectionExtCollection() : base(StringComparer.OrdinalIgnoreCase) { }
}

public class InfSectionEntryCollection : Dictionary<string, Entry>
{
	public InfSectionEntryCollection() : base(StringComparer.OrdinalIgnoreCase) { }
}
