using InfHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace InfHelperTests;

[TestClass()]
public class InfHelperTypedParsingTests
{
	private const string testFolder = "infs";

	[TestMethod()]
	public void NoDefaultStrings()
	{
		var content = File.ReadAllText(Path.Combine(testFolder, "nodefaultstrings.inf"));
		var data = InfUtil.Parse(content);
		var result = InfUtil.SecondLevelParse(data);

		Assert.AreEqual(1, result.Strings.Count());
		Assert.AreEqual("Strings.0000", result.Strings.First().Localization);
	}

	[TestMethod()]
	public void LocalizedStrings()
	{
		var content = File.ReadAllText(Path.Combine(testFolder, "localizedstrings.inf"));
		var data = InfUtil.Parse(content);
		var result = InfUtil.SecondLevelParse(data);

		Assert.AreEqual(2, result.Strings.Count());
		Assert.IsNull(result.Strings.First().Localization);
		Assert.AreEqual("Strings.0000", result.Strings.Last().Localization);
	}
}
