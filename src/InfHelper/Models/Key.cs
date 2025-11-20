using System;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models;

[Obsolete("Refactor all uses of this class to use Dictionary<string, List<KeyValue>> instead.")]
public class Key
{
	/// <summary>
	/// Id of the key, can be empty (anonymous keys) 
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Values separated by comma, can be inside quotes e.g. "value"
	/// </summary>
	public List<KeyValue> KeyValues { get; set; } = [];

	public string PrimitiveValue => string.Join(", ", KeyValues.Select(x => x.PrimitiveValue));
}