using System;

namespace InfHelper.Exceptions;

public class RequiredEntryMissingException(string sectionName, string propertyName) : Exception($"INF is missing required entry: {sectionName}:{propertyName}");
