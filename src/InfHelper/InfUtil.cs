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
		parser.CategoryDiscovered += (sender, category) =>
		{
			var existingCategory = infData.Categories
				.FirstOrDefault(x => x.IsNamed(category.Name));

			if (existingCategory is null)
			{
				infData.Categories.Add(category);
				return;
			}

			// Merge keys
			foreach (var key in category.Keys)
			{
				var existingKey = existingCategory.Keys
					.FirstOrDefault(x => string.Equals(x.Id, key.Id, StringComparison.OrdinalIgnoreCase));

				if (existingKey is null)
				{
					existingCategory.Keys.Add(key);
					continue;
				}

				existingKey.KeyValues = key.KeyValues;
			}

		};
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

	internal static IEnumerable<string> GeneratedDectoratedSections(IEnumerable<KeyValue> keyValues)
	{
		if (keyValues.Any(x => x.IsDynamic))
		{
			throw new InvalidOperationException("Cannot generate decorated section names for dynamic key values.");
		}

		var baseSectionName = keyValues.First();
		yield return baseSectionName.Value;

		foreach (var dectoratedSection in keyValues.Skip(1))
		{
			yield return $"{baseSectionName.Value}.{dectoratedSection.Value}";
		}
	}

	public static T SerializeInto<T>(string data, out InfData outputData) where T : new()
	{
		var o = new T();
		var t = o.GetType();
		var infData = new InfData();
		var parser = new ContentParser();

		var dict = new Dictionary<Key, PropertyInfo>();

		parser.CategoryDiscovered += (sender, category) =>
		{
#warning add merge code
			infData.Categories.Add(category);
			foreach (var property in t.GetProperties())
			{
				if (Attribute.IsDefined(property, typeof(InfKeyValue)))
				{
					if (Attribute.GetCustomAttribute(property, typeof(InfKeyValue)) is not InfKeyValue attribute)
					{
						throw new InvalidOperationException("Attribute InfKeyValue not found on property " + property.Name);
					}

					if (category.IsNamed(attribute.CategoryId))
					{
						var key = category[attribute.KeyId];
						if (key != null)
						{
							property.SetValue(o, key.PrimitiveValue);

							//save dynamic values for further dereferencing
							if (attribute.DeferenceDynamicValueKeys && 
								key.KeyValues.Count != 0 &&
								key.KeyValues.Any(x => x.IsDynamic))
							{
								// save for later des.
								dict.Add(key, property);
							}
						}
					}
				}
			}
		};
		parser.Parse(data);
		outputData = infData;

		//dereference keys - if some left after category dereferencing
		if (dict.Count != 0)
		{
			DerefereneDynamicKeys(o, infData, dict);
		}

		return o;
	}

	private static void DerefereneDynamicKeys<T>(T o, InfData infData, Dictionary<Key, PropertyInfo> dict) where T : new()
	{
		foreach (var item in dict)
		{
			string value = GetPrimitiveValueForKey(infData, item.Key);
			if (value != null)
			{
				item.Value.SetValue(o, value);
			}
		}
	}

	private static string GetPrimitiveValueForKey(InfData data, Key key)
	{
		if (key.KeyValues.Count != 0)
		{
			var first = key.KeyValues.First();
			//dynamic
			if (first.IsDynamic)
			{
				if (first.DynamicKeyId is null)
				{
					throw new InvalidOperationException("Dynamic key value does not have a DynamicKeyId set.");
				}

				return data.FindKeyById(first.DynamicKeyId) //find dynamic key
					.First(x => x.KeyValues.All(v => !v.IsDynamic)) // that has not a dynamic value
					.KeyValues.First().Value ?? //return the first text value
					throw new InvalidOperationException("Key value for dynamic key was null."); 
			}
			//static
			return key.PrimitiveValue;
		}
		return "";
	}

	public static T SerializeFileInto<T>(string path, out InfData outputData) where T : new()
	{
		var content = File.ReadAllText(path);
		return SerializeInto<T>(content, out outputData);
	}
}
