using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace InfHelper.Models.Sections;

/// <summary>
/// Represents a strongly-typed section within an INF file, providing access to its entries as a sequence of elements of
/// type T.
/// </summary>
/// <remarks>Use this type to enumerate the entries of a specific INF section. The section name is available via
/// the <see cref="Name"/> property.</remarks>
/// <typeparam name="T">The type of section entry, which must implement <see cref="IInfSection{T}"/>.</typeparam>
public sealed record InfSection<T> : IReadOnlyDictionary<string, T> where T : IInfSection<T>
{
	/// <summary>
	/// Initializes a new instance of the InfSection class with the specified section name, collection of all sections, and
	/// entry collection.
	/// </summary>
	/// <param name="name">The name of the section to be represented by this instance. Cannot be null.</param>
	/// <param name="allSections">A collection containing all sections available for reference. Cannot be null.</param>
	/// <param name="entries">A collection of entries associated with this section. Cannot be null.</param>
	internal InfSection(string name, InfSectionCollection allSections, InfSectionEntryCollection entries)
	{
		Name = name;
		_entryDictionary = entries
			.ToDictionary(entry => entry.Key, entry => T.Create(name, entry.Value, allSections));
	}

	private readonly Dictionary<string, T> _entryDictionary;

	/// <summary>
	/// Gets the name of this section.
	/// </summary>
	public string Name { get; init; }

    public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, T>)_entryDictionary).Keys;

    public IEnumerable<T> Values => ((IReadOnlyDictionary<string, T>)_entryDictionary).Values;

    public int Count => ((IReadOnlyCollection<KeyValuePair<string, T>>)_entryDictionary).Count;

    public T this[string key] => ((IReadOnlyDictionary<string, T>)_entryDictionary)[key];

    public bool ContainsKey(string key)
    {
        return ((IReadOnlyDictionary<string, T>)_entryDictionary).ContainsKey(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
    {
        return ((IReadOnlyDictionary<string, T>)_entryDictionary).TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, T>>)_entryDictionary).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_entryDictionary).GetEnumerator();
    }
}
