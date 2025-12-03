using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using InfHelper.Models;
using InfHelper.Models.Attributes;
using InfHelper.Parsers;

namespace InfHelper;

public static class InfUtil
{
	public static InfData Parse(string data)
	{
		var infData = new InfData();
		var parser = new ContentParser();
		parser.CategoryDiscovered += (sender, category) => infData.Sections.Add(category);
		parser.Parse(data);
		return infData;
	}

	public static InfData ParseFile(string path)
	{
		var content = File.ReadAllText(path);
		return Parse(content);
	}

	public static TypedInfData SecondLevelParse(InfData data)
	{
		return TypedInfData.Parse(data);


   //	 return new TypedInfData()
   //	 {
   //		 Version = new VersionSection(versionCategory.Keys),
   //		 Manufacturers = manufacturers ?? [],
   //		 SourceDiskNames = sourceDiskNamesCategory is null ? null : new SourceDiskNamesSection(sourceDiskNamesCategory.Keys),
   //		 SourceDiskFiles = sourceDiskFilesCategory is null ? null : new SourceDiskFilesSection(sourceDiskFilesCategory.Keys),
			//Strings = new StringsSection(stringsCategory.Keys),
   //		 OtherCategories = remainingCategories
   //	 };
	}

	public static T SerializeInto<T>(string data, out InfData outputData) where T : new()
	{
		var o = new T();
		var t = o.GetType();
		var infData = new InfData();
		var parser = new ContentParser();

		var dict = new Dictionary<IEnumerable<EntryValue>, PropertyInfo>();

		parser.CategoryDiscovered += (sender, category) =>
		{
			infData.Sections.Add(category);
			foreach (var property in t.GetProperties())
			{
				if (Attribute.GetCustomAttribute(property, typeof(InfKeyValue)) is not InfKeyValue attribute)
				{
					throw new InvalidOperationException("Attribute InfKeyValue not found on property " + property.Name);
				}

				if (!Attribute.IsDefined(property, typeof(InfKeyValue)) || 
					!category.IsNamed(attribute.CategoryId))
				{
					continue;
				}

				var values = category
					.Where(x => string
						.Equals(x.Name, attribute.KeyId, StringComparison.OrdinalIgnoreCase))
					.SelectMany(x => x.Values);
						
				if (values is null)
				{
					continue; 
				}

				property.SetValue(o, GetPrimitiveValueForEntryValues(values));

				//save dynamic values for further dereferencing
				if (attribute.DeferenceDynamicValueKeys && 
					values.Any(x => x.IsDynamic))
				{
					// save for later des.
					dict.Add(values, property);
				}
			}
		};
		parser.Parse(data);
		outputData = infData;

		//dereference keys - if some left after category dereferencing
		if (dict.Count != 0)
		{
			ResolveDynamicKeys(o, infData, dict);
		}

		return o;
	}

	private static string GetPrimitiveValueForEntryValues(IEnumerable<EntryValue> entryValues) => 
		string.Join(", ", entryValues.Select(x => x.PrimitiveValue));

	private static void ResolveDynamicKeys<T>(T o, InfData infData, Dictionary<IEnumerable<EntryValue>, PropertyInfo> dict) where T : new()
	{
		foreach (var item in dict)
		{
			string value = GetPrimitiveValueForEntry(infData, item.Key);
			if (value != null)
			{
				item.Value.SetValue(o, value);
			}
		}
	}

	private static string GetPrimitiveValueForEntry(InfData data, IEnumerable<EntryValue> entryValues)
	{
		// The entry has no values, return empty string
		if (!entryValues.Any())
		{
			return string.Empty;
		}

		// Get the first value, which would be enclosed in '%' if dynamic
		var firstValue = entryValues.First();

		// The entry is not dynamic, return the primitive value directly
		if (!firstValue.IsDynamic)
		{
			return GetPrimitiveValueForEntryValues(entryValues);
		}

		// The entry is dynamic, ensure it has a DynamicKeyId
#warning refactor DynamicKeyId to not be nullable
		if (firstValue.DynamicKeyId is null)
		{
			throw new InvalidOperationException("Dynamic key value does not have a DynamicKeyId set.");
		}

		return data.FindEntryById(firstValue.DynamicKeyId) // Find an entry that matches the DynamicKeyId
			.Values.FirstOrDefault()?.Value // Get the first value of that entry
			?? throw new InvalidOperationException("Key value for dynamic key was null."); 
	}

	public static T SerializeFileInto<T>(string path, out InfData outputData) where T : new()
	{
		var content = File.ReadAllText(path);
		return SerializeInto<T>(content, out outputData);
	}
}
