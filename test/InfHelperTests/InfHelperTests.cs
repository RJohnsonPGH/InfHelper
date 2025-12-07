using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using InfHelper;
using InfHelper.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfHelperTests;

[TestClass()]
public class InfHelperTests
{
	private const string testFolder = "infs";

	[TestMethod()]
	public void ParseTest()
	{
		var content = File.ReadAllText(Path.Combine(testFolder, "oem100.inf"));
		var data = InfUtil.BasicParse(content);

		// random key and key value
		Assert.AreEqual("\"$WINDOWS NT$\"", data["Version"]
			.Where(x => string.Equals(x.Name, "Signature", StringComparison.OrdinalIgnoreCase))
			.Single()
			.Values
			.GetPrimitiveValue());
		// anonymous key
		Assert.AreEqual("RazerCoinstaller.dll", data["Razer_CoInstaller_CopyFiles"]
			.Entries[0]
			.Values
			.GetPrimitiveValue());

		//anonymous key with multiple values
		var values = new HashSet<string> { "HKR", null, "CoInstallers32", "0x00010000", "RazerCoinstaller.dll,RazerCoinstaller" };
		Assert.IsTrue(data["Razer_CoInstaller_AddReg"].Entries.First().Values.All(x => values.Contains(x.Value)));

		Assert.AreEqual(17, data.Sections.Count);
	}

	[TestMethod()]
	public void ParseCaseInsensitiveTest()
	{
		var content = File.ReadAllText(Path.Combine(testFolder, "oem100.inf"));
		var data = InfUtil.BasicParse(content);

		// random key and key value
		Assert.AreEqual("\"$WINDOWS NT$\"", data["vErSiOn"]
			.Where(x => string.Equals(x.Name, "SIGnatURE", System.StringComparison.OrdinalIgnoreCase))
			.Single()
			.Values
			.GetPrimitiveValue());
		// anonymous key
		Assert.AreEqual("RazerCoinstaller.dll", data["RAzEr_CoInstaller_COpyFiles"]
			.Entries
			.First()
			.Values
			.GetPrimitiveValue());

		//anonymous key with multiple values
		var values = new HashSet<string> { "HKR", null, "CoInstallers32", "0x00010000", "RazerCoinstaller.dll,RazerCoinstaller" };
		Assert.IsTrue(data["Razer_CoInstaller_AddReg"]
			.Entries
			.First()
			.Values
			.All(x => values.Contains(x.Value)));
	}

	[TestMethod()]
	public void FileParserEndpointTest()
	{
		var sw = new Stopwatch();
		var files = Directory.GetFiles(testFolder);
		foreach (var file in files)
		{
			sw.Reset();
			Trace.WriteLine("Parsing file: " + file);
			sw.Start();
			InfUtil.BasicParseFile(file);
			sw.Stop();
			Trace.WriteLine($"Completed. Elapsed time: {sw.Elapsed}");
		}
	}

	[TestMethod()]
	public void PureValueParsingTest()
	{
		string formula =
			"[DestinationDirs]\r\n" + 
			"Razer_CoInstaller_CopyFiles = 11\r\n" +
			"Razer_Installer_CopyFiles = 16422,\"Razer\\RzWizardPkg\"\r\n" +
			"Razer_Installer_CopyFilesWOW64 = 16426,\"Razer\\RzWizardPkg\"\r\n" +
			"Razer_Installer_CopyFilesWithBrackets = 16428,\"Razer\\RzWizardPkg ; [Brackets=X]\"";
		var data = InfUtil.BasicParse(formula);
		Assert.AreEqual("11", data["DestinationDirs"]
			.Where(x => string.Equals(x.Name, "Razer_CoInstaller_CopyFiles", StringComparison.OrdinalIgnoreCase))
			.Single()
			.Values
			.GetPrimitiveValue());
		Assert.AreEqual("16422, \"Razer\\RzWizardPkg\"", data["DestinationDirs"]
			.Where(x => string.Equals(x.Name, "Razer_Installer_CopyFiles", StringComparison.OrdinalIgnoreCase))
			.Single()
			.Values
			.GetPrimitiveValue());
		Assert.AreEqual("16426, \"Razer\\RzWizardPkg\"", data["DestinationDirs"]
			.Where(x => string.Equals(x.Name, "Razer_Installer_CopyFilesWOW64", StringComparison.OrdinalIgnoreCase))
			.Single()
			.Values
			.GetPrimitiveValue());
		Assert.AreEqual("16428, \"Razer\\RzWizardPkg ; [Brackets=X]\"", data["DestinationDirs"]
			.Where(x => string.Equals(x.Name, "Razer_Installer_CopyFilesWithBrackets", StringComparison.OrdinalIgnoreCase))
			.Single()
			.Values
			.GetPrimitiveValue());
	}

	[TestMethod()]
	public void QuotedKeyParsingTest()
	{
		string formula =
			"[AzaliaManufacturerID.NTamd64.10.0...15063]\r\n\"Realtek High Definition Audio\" = IntcAzAudModel, HDAUDIO\\FUNC_01&VEN_10EC&DEV_0257&SUBSYS_17AA39F5 ; ThinkBook 16p NX ARH\r\n";
		var data = InfUtil.BasicParse(formula);
		Assert.AreEqual("IntcAzAudModel, HDAUDIO\\FUNC_01&VEN_10EC&DEV_0257&SUBSYS_17AA39F5", 
			data["AzaliaManufacturerID.NTamd64.10.0...15063"]
			.Where(x => string.Equals(x.Name, "Realtek High Definition Audio", StringComparison.OrdinalIgnoreCase))
			.Single()
			.Values
			.GetPrimitiveValue());
	}

	[TestMethod()]
	public void SearchMethdTest()
	{
		string formula =
			"[DestinationDirs]\r\nRazer_CoInstaller_CopyFiles = 11 ; Comment\r\nRazer_Installer_CopyFiles = 16422,\"Razer\\RzWizardPkg\"\r\nRazer_Installer_CopyFilesWOW64 = 16426,\"Razer\\RzWizardPkg\"";
		var data = InfUtil.BasicParse(formula);
		Assert.AreEqual("11", data
			.FindEntryById("Razer_CoInstaller_CopyFiles")
			.Values
			.GetPrimitiveValue());
	}
	
	[TestMethod()]
	public void CanParseSpacesInCategoryName()
	{
		var info = InfUtil.BasicParseFile(Path.Combine(testFolder, "spaces.inf"));
		
		// info.Categories should contain [OEM URLS]
		Assert.AreEqual(1, info.Sections.Count(x => x.Name == "OEM URLS"));
	}

	[TestMethod()]
	public void AssumeNewLinesEndQuote()
	{
		var data = InfUtil.BasicParseFile(Path.Combine(testFolder, "oem137.inf"));

		var strings = data.Sections.FirstOrDefault(c => c.Name == "Strings");

		Assert.AreEqual("\"{DFF21BE1-F70F-11D0-B917-00A0C9223196}\"", strings.Entries
			.Where(k => k.Name == "KSNODETYPE_MICROPHONE")
			.First()
			.Values
			.GetPrimitiveValue());

		Assert.AreEqual("\"{17CCA71B-ECD7-11D0-B908-00A0C9223196}\"", strings.Entries
			.Where(k => k.Name == "Proxy.CLSID")
			.First()
			.Values
			.GetPrimitiveValue());
	}

	[TestMethod()]
	public void EmptySeparatorsAreNull()
	{
		var info = InfUtil.BasicParseFile(Path.Combine(testFolder, "oem136.inf"));

		var sourceDisksSection = info.Sections.FirstOrDefault(c => c.Name == "SourceDisksNames");

		Assert.AreEqual(4, sourceDisksSection.Entries.First().Values.Count);
		Assert.AreEqual("\\", sourceDisksSection.Entries.First().Values.Last().Value);
	}
}