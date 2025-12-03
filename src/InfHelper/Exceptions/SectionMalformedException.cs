using System;

namespace InfHelper.Exceptions;

public class SectionMalformedException(string sectionName) :
	Exception($"INF section is malformed: {sectionName}");
