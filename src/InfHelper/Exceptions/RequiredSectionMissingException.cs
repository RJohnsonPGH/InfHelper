using System;

namespace InfHelper.Exceptions;

public class RequiredSectionMissingException(string sectionName, string sectionNameExt) : 
	Exception($"INF is missing required section: {sectionName}.{sectionNameExt}");
