using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models;

public sealed record Entry(string Name, List<EntryValue> Values) : IEnumerable<EntryValue>
{
    public bool IsNamed(string name) =>
		string.Equals(Name, name, StringComparison.OrdinalIgnoreCase);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public IEnumerator<EntryValue> GetEnumerator() => Values.GetEnumerator();

	public override string ToString() => Values.GetPrimitiveValue();
	public static implicit operator string(Entry entry) => entry.ToString();
}

public class EntryValue
{
	public virtual required string Value { get; set; }
	public virtual string PrimitiveValue => Value;
	public virtual bool IsDynamic => Value[0] == '%' && Value[^1] == '%';
	public virtual string? DynamicKeyId => IsDynamic && Value?.Length > 0 ? Value[1..^1] : null;
}

public class PureValue : EntryValue
{
	public override string PrimitiveValue => $"\"{Value}\"";
}
public static class EntryValueExtensions
{
	public static string GetPrimitiveValue(this IEnumerable<EntryValue> values) =>
		string.Join(", ", values.Select(x => x.PrimitiveValue));
}