using InfHelper;
using InfHelper.Exceptions;
using InfHelper.Models;
using InfHelper.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace InfHelperTests.Parsers;

[TestClass()]
public class ContentParserTests
{
	private const string testFolder = "infs";

	[TestMethod()]
	public void CategoryParsing()
	{
		string test = "[CATEGORY]";
		var categories = new List<Section>();
		var parser = new ContentParser();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);

		parser.Parse(test);

		Assert.AreEqual(1, categories.Count);
		Assert.AreEqual("CATEGORY", categories.First().Name);
	}

	[TestMethod()]
	public void CategoryWithSlashParsing()
	{
		string test = "[CATEGOR\\Y]";
		var categories = new List<Section>();
		var parser = new ContentParser();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);

		parser.Parse(test);

		Assert.AreEqual(1, categories.Count);
		Assert.AreEqual("CATEGOR\\Y", categories.First().Name);
	}

	[TestMethod()]
	[ExpectedException(typeof(InvalidTokenException), @"'\' are not allowed as the last token in a Category")]
	public void CategoryWithSlashAsLastCharacterParsing()
	{
		string test = "[CATEGORY\\]";
		var categories = new List<Section>();
		var parser = new ContentParser();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);

		parser.Parse(test);
	}

	[TestMethod()]
	public void MultipleCategoryParsing()
	{
		int CATEGORIES_COUNT = new Random().Next(1, 10);
		var testCategories = new List<string>();

		for (int i = 1; i <= CATEGORIES_COUNT; i++)
		{
			testCategories.Add($"[CATEGORY{i}]");
		}

		string test = string.Join(" \n ", testCategories);

		var categories = new List<Section>();
		var parser = new ContentParser();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);

		parser.Parse(test);

		Assert.AreEqual(CATEGORIES_COUNT, categories.Count);

		for (var i = 1; i <= CATEGORIES_COUNT; i++)
		{
			Assert.IsTrue(categories.Any(x => x.Name == $"CATEGORY{i}"));
		}
	}

	[TestMethod()]
	public void SimpleCategoryWithSimpleKey()
	{
		string formula = "[Category] \n Key = Value";
		var parser = new ContentParser();

		var categories = new List<Section>();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);
		parser.Parse(formula);

		Assert.AreEqual("Category", categories.First().Name);
		var entry = categories.First().Entries.First();

		Assert.AreEqual("Key", entry.Name); // StringComparison.Ordinal);
		Assert.AreEqual(1, entry.Values.Count);
		Assert.AreEqual("Value", entry.Values[0].Value); // StringComparison.Ordinal);
	}

	[TestMethod()]
	public void SimpleCategoryWithMultipleSimpleKeys()
	{
		string formula = "[Category]";// \n Key = Value \n Key1 = Value1 \n Key2 = Value2 \n Key3 = Value3";
		int Keys_Count = 4;
		for (int i = 0; i < Keys_Count; i++)
		{
			formula = string.Concat(formula, $" \n Key{i} = Value{i}");
		}
		var parser = new ContentParser();

		var categories = new List<Section>();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);
		parser.Parse(formula);

		var firstCategory = categories.First();

		Assert.AreEqual("Category", firstCategory.Name);
		Assert.AreEqual(4, firstCategory.Entries.Count);

		int j = 0;
		foreach (var currentEntry in firstCategory.Entries)
		{
			Assert.AreEqual($"Key{j}", currentEntry.Name);
			Assert.AreEqual($"Value{j}", currentEntry.Values[0].Value);
			j++;
		}
	}

	[TestMethod()]
	public void SimpleCategoryWithMultipleSimpleKeysReal()
	{
		string formula =
			"[Install_MPCIEX_GENM2_D_REV_59_7265_BGN_2x2_HMC_WINT_64_BGN15.Services]\r\nInclude		 = netvwifibus.inf\r\nNeeds		   = VWiFiBus.Services";

		var parser = new ContentParser();

		var sections = new List<Section>();
		parser.CategoryDiscovered += (sender, section) => sections.Add(section);
		parser.Parse(formula);

		var firstSection = sections.First();

		Assert.AreEqual(2, firstSection.Entries.Count);
		Assert.AreEqual("Install_MPCIEX_GENM2_D_REV_59_7265_BGN_2x2_HMC_WINT_64_BGN15.Services", firstSection.Name);

		Assert.AreEqual("Include", firstSection.Entries[0].Name);
		Assert.AreEqual("Needs", firstSection.Entries[1].Name);

		Assert.AreEqual("netvwifibus.inf", firstSection.Entries[0].Values[0].Value);
		Assert.AreEqual("VWiFiBus.Services", firstSection.Entries[1].Values[0].Value);
	}

	[TestMethod()]
	public void SimpleCategoryWithOneMultiValueKey()
	{
		string formula =
			"[Install_MPCIEX_GENM2_D_REV_61_7265_BGN_2x2_HMC_WINT_64_BGN15.Services] \r\n AddService	 = Netwtw04, 2, NIC_Service_WINT_64, Common_EventLog_WINT_64";

		var parser = new ContentParser();

		var categories = new List<Section>();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);
		parser.Parse(formula);

		var firstCategory = categories.First();

		Assert.AreEqual("Install_MPCIEX_GENM2_D_REV_61_7265_BGN_2x2_HMC_WINT_64_BGN15.Services", firstCategory.Name);
		Assert.AreEqual("AddService", firstCategory.Entries.First().Name);
		var values = firstCategory.Entries.First().Values.ToList();

		Assert.AreEqual("Netwtw04", values[0].Value);
		Assert.AreEqual("2", values[1].Value);
		Assert.AreEqual("NIC_Service_WINT_64", values[2].Value);
		Assert.AreEqual("Common_EventLog_WINT_64", values[3].Value);
	}

	[TestMethod()]
	public void SimpleCategoryWithMultipleMultiValueKeys()
	{
		string formula =
			"[Intel.NTAMD64.6.2]\r\n%IntcAudDeviceDesc% = IntcAudModel, HDAUDIO\\FUNC_01&VEN_8086&DEV_2809&SUBSYS_80860101, HDAUDIO\\FUNC_01&VEN_8086&DEV_2809\r\n%IntcAudDeviceDesc% = IntcAudModel, " +
			"INTELAUDIO\\FUNC_01&VEN_8086&DEV_2809&SUBSYS_80860101, INTELAUDIO\\FUNC_01&VEN_8086&DEV_2809\r\n%IntcAudDeviceDesc% = IntcAudModel, HDAUDIO\\FUNC_01&VEN_8086&DEV_280A&SUBSYS_80860101, HDAUDIO\\FUNC_01&VEN_8086&DEV_280A";

		var parser = new ContentParser();

		var categories = new List<Section>();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);
		parser.Parse(formula);

		Assert.AreEqual(1, categories.Count);
		var firstCategory = categories.First();

		Assert.AreEqual("Intel.NTAMD64.6.2", firstCategory.Name);
		Assert.AreEqual(3, firstCategory.Entries.Count);

		//first key
		var entry = firstCategory.Entries.First();
		var values = entry.Values.ToList();
		Assert.AreEqual("%IntcAudDeviceDesc%", entry.Name);
		Assert.AreEqual("IntcAudModel", values[0].Value);
		Assert.AreEqual("HDAUDIO\\FUNC_01&VEN_8086&DEV_2809&SUBSYS_80860101", values[1].Value);
		Assert.AreEqual("HDAUDIO\\FUNC_01&VEN_8086&DEV_2809", values[2].Value);

		//second key
		entry = firstCategory.Entries.Skip(1).First();
		values = [.. entry.Values];
		Assert.AreEqual("%IntcAudDeviceDesc%", entry.Name);
		Assert.AreEqual("IntcAudModel", values[0].Value);
		Assert.AreEqual("INTELAUDIO\\FUNC_01&VEN_8086&DEV_2809&SUBSYS_80860101", values[1].Value);
		Assert.AreEqual("INTELAUDIO\\FUNC_01&VEN_8086&DEV_2809", values[2].Value);

		//third key
		entry = firstCategory.Entries.Skip(2).First();
		values = [.. entry.Values];
		Assert.AreEqual("%IntcAudDeviceDesc%", entry.Name);
		Assert.AreEqual("IntcAudModel", values[0].Value);
		Assert.AreEqual("HDAUDIO\\FUNC_01&VEN_8086&DEV_280A&SUBSYS_80860101", values[1].Value);
		Assert.AreEqual("HDAUDIO\\FUNC_01&VEN_8086&DEV_280A", values[2].Value);
	}

	[TestMethod]
	public void RealLifeFileTest()
	{
		var sw = new Stopwatch();
		var files = Directory.GetFiles(testFolder);
		var parser = new ContentParser();
		foreach (var file in files)
		{
			sw.Reset();
			Trace.WriteLine("Loading file content: " + file);
			var content = File.ReadAllText(file);
			Trace.WriteLine("Parsing file: " + file);
			sw.Start();
			parser.Parse(content);
			sw.Stop();
			Trace.WriteLine($"Completed. Elapsed time: {sw.Elapsed}");
		}
	}
	[TestMethod]
	public void RealLifeFileTestDeep()
	{
		var content = File.ReadAllText(Path.Combine(testFolder, "oem100.inf"));
		var parser = new ContentParser();
		var categories = new List<Section>();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);
		parser.Parse(content);

		//Check categories names
		Assert.AreEqual("Version", categories[0].Name);
		Assert.AreEqual("DestinationDirs", categories[1].Name);
		Assert.AreEqual("Manufacturer", categories[2].Name);
		Assert.AreEqual("Standard", categories[3].Name);
		Assert.AreEqual("Standard.NTAMD64", categories[4].Name);
		Assert.AreEqual("Razer.NTx86", categories[5].Name);
		Assert.AreEqual("Razer.NTAMD64", categories[6].Name);
		Assert.AreEqual("Razer.NTAMD64.HW", categories[7].Name);
		Assert.AreEqual("Razer.NTAMD64.Services", categories[8].Name);
		Assert.AreEqual("SourceDisksNames", categories[9].Name);
		Assert.AreEqual("SourceDisksFiles", categories[10].Name);
		Assert.AreEqual("Razer.NTAMD64.CoInstallers", categories[11].Name);
		Assert.AreEqual("Razer_CoInstaller_AddReg", categories[12].Name);
		Assert.AreEqual("Razer_CoInstaller_CopyFiles", categories[13].Name);
		Assert.AreEqual("Razer_Installer_CopyFiles", categories[14].Name);
		Assert.AreEqual("Razer_Installer_CopyFilesWOW64", categories[15].Name);
		Assert.AreEqual("Strings", categories[16].Name);
	}

	[TestMethod]
	public void SourceDiskFileWithSpaces()
	{
		var content = File.ReadAllText(Path.Combine(testFolder, "sourcediskspaces.inf"));
		var parser = new ContentParser();
		var categories = new List<Section>();
		parser.CategoryDiscovered += (sender, category) => categories.Add(category);
		parser.Parse(content);

		var sourceDiskFiles = categories.Where(c => c.Name == "SourceDisksFiles").ToList();
		Assert.AreEqual("Dep With Space.txt", sourceDiskFiles[0].Entries.First().Name);
		Assert.AreEqual("file2.txt", sourceDiskFiles[0].Entries.Last().Name);
	}
}