using System;
using System.Collections.Generic;

namespace InfHelper.Models.Sections;

public class InfSectionCollection : Dictionary<string, InfSectionExtCollection>
{
	public InfSectionCollection() : base(StringComparer.OrdinalIgnoreCase) { }
}

public class InfSectionExtCollection : Dictionary<string, InfSectionExtEntryCollection>
{
	public InfSectionExtCollection() : base(StringComparer.OrdinalIgnoreCase) { }
}

public class InfSectionExtEntryCollection : Dictionary<string, Entry>
{
	public InfSectionExtEntryCollection() : base(StringComparer.OrdinalIgnoreCase) { }
}
