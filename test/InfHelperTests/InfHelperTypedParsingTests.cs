using InfHelper;
using InfHelperTests.TestData;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace InfHelperTests;

public class InfHelperTypedParsingTests
{
	private const string testFolder = "infs";

	[Theory]
	[ClassData(typeof(VersionTestData))]
	public void ParseInfVersion_ShouldMatch(string filename, string signature, string classType, Guid classGuid, string provider, 
		DateOnly driverVerDate, Version driverVerVersion)
	{
		// Arrange
		var content = File.ReadAllText(Path.Combine(testFolder, filename));
		
		// Act
		var data = InfUtil.Parse(content);
		var result = InfUtil.SecondLevelParse(data);

		// Assert
		Assert.Equal(signature, result.Version.Signature);
		Assert.Equal(classType, result.Version.Class);
		Assert.Equal(classGuid, result.Version.ClassGuid);
		Assert.Equal(provider, result.Version.Provider);
		Assert.Equal(driverVerDate, result.Version.DriverVer.Date);
		Assert.Equal(driverVerVersion, result.Version.DriverVer.Version);
	}

	[Theory]
	[ClassData(typeof(ManufacturerTestData))]
	public void ParseInfManufacturer_ShouldMatch(string filename, int entryCount, string entryName)
	{
		// Arrange
		var content = File.ReadAllText(Path.Combine(testFolder, filename));

		// Act
		var data = InfUtil.Parse(content);
		var result = InfUtil.SecondLevelParse(data);

		// Assert
		var manufacturers = result.Manufacturers.ToList();
		Assert.Equal(entryCount, manufacturers.Count);

		var manufacturer = manufacturers.SingleOrDefault(manufacturer => string.Equals(manufacturer.Name, entryName, StringComparison.OrdinalIgnoreCase));
		Assert.NotNull(manufacturer);
	}

	[Theory]
	[ClassData(typeof(ModelTestData))]
	public void ParseInfModel_ShouldMatch(string filename, int modelCount, string[] modelExtensionNames, string[] modelNames)
	{
		// Arrange
		var content = File.ReadAllText(Path.Combine(testFolder, filename));

		// Act
		var data = InfUtil.Parse(content);
		var result = InfUtil.SecondLevelParse(data);
		var manufacturers = result.Manufacturers.ToList();
		var models = manufacturers[0].Models.ToList();

		// Assert
		Assert.Equal(modelCount, models.Count);
		Assert.Equal(models.Select(x => x.Extension).ToArray(), modelExtensionNames);
		Assert.Equal(models.Select(x => x.Name).ToArray(), modelNames);
	}
}
