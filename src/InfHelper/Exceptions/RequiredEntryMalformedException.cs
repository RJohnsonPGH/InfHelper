using System;

namespace InfHelper.Exceptions;

public  class RequiredEntryMalformedException(string sectionName, string entryName, string entryValue) : 
	Exception($"INF entry is malformed: {sectionName}:{entryName}:{entryValue}"); 
