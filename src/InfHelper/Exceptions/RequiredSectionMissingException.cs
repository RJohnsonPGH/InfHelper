using System;

namespace InfHelper.Exceptions;

public class RequiredSectionMissingException(string sectionName) : Exception($"INF is missing required section: {sectionName}");
