using Xunit;

namespace InfHelperTests.TestData;

public class ManufacturerTestData : TheoryData<string, int, string>
{
	public ManufacturerTestData()
	{
		Add(
			"spaces.inf",
			1,
			"%Manufact%"
		);
	}
}
