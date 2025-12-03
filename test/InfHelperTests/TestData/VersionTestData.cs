using System;
using Xunit;

namespace InfHelperTests.TestData;

public class VersionTestData : TheoryData<string, string, string, Guid, string, DateOnly, Version>
{
	public VersionTestData()
	{
		Add(
			"spaces.inf",
			@"""$Windows NT$""",
			"Printer",
			Guid.Parse("{4D36E979-E325-11CE-BFC1-08002BE10318}"),
			"%Provider%",
			DateOnly.Parse("12/27/2012"),
			Version.Parse("01.14.01.00")
		);

		Add(
			"oem4.inf",
			@"""$CHICAGO$""",
			"MEDIA",
			Guid.Parse("{4d36e96c-e325-11ce-bfc1-08002be10318}"),
			"%NV%",
			DateOnly.Parse("05/28/2017"),
			Version.Parse("3.90.1")
		);

		Add(
			"oem100.inf",
			@"""$WINDOWS NT$""",
			"HIDClass",
			Guid.Parse("{745a17a0-74d3-11d0-b6fe-00a0c90f57da}"),
			"%Razer%",
			DateOnly.Parse("12/01/2013"),
			Version.Parse("6.2.9200.16425")
		);
	}
}
