using Xunit;

namespace InfHelperTests.TestData;

public class ModelTestData : TheoryData<string, int, string[], string[]>
{
	public ModelTestData()
	{
		Add("spaces.inf",
			4,
			["NTAMD64", "NTAMD64", "NTAMD64", "NTAMD64"],
			["Brother HL-4040CN series", "Brother HL-4050CDN series", "Brother HL-4070CDW series", "Brother HL-4040CDN series"]);
		Add("oem4.inf",
			3,
			[string.Empty, "NTamd64", "NTx86"],
			["%NVVAD_WaveExtensible.DeviceDesc%", "%NVVAD_WaveExtensible.DeviceDesc%", "%NVVAD_WaveExtensible.DeviceDesc%"]);
		Add("oem100.inf",
			2,
			[string.Empty, "NTAMD64"],
			["%Razer0502.DeviceDesc%", "%Razer0502.DeviceDesc%"]);
	}
}
