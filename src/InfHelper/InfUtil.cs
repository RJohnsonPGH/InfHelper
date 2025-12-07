using System.IO;
using InfHelper.Models;
using InfHelper.Parsers;

namespace InfHelper;

public static class InfUtil
{
	public static InfData BasicParse(string data)
	{
		var infData = new InfData();
		var parser = new ContentParser();
		parser.CategoryDiscovered += (sender, category) => infData.Sections.Add(category);
		parser.Parse(data);
		return infData;
	}

	public static InfData BasicParseFile(string path)
	{
		var content = File.ReadAllText(path);
		return BasicParse(content);
	}

	public static TypedInfData Parse(InfData data)
	{
		return TypedInfData.Parse(data);
	}
}
