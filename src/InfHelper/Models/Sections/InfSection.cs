using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections;

/// <summary>
/// Represents a strongly-typed section within an INF file, providing access to its entries as a sequence of elements of
/// type T.
/// </summary>
/// <remarks>Use this type to enumerate the entries of a specific INF section. The section name is available via
/// the <see cref="Name"/> property.</remarks>
/// <typeparam name="T">The type of section entry, which must implement <see cref="IInfSection{T}"/>.</typeparam>
public sealed record InfSection<T> : IEnumerable<T> where T : IInfSection<T>
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
		_branches = entries
				.Select(entry => T.Create(name, entry.Value, allSections));
	}

	/// <summary>
	/// Gets the name of this section.
	/// </summary>
	public string Name { get; init; }

	// IEnumerable implementation - allows for iteration over the contained T entries
	private readonly IEnumerable<T> _branches;
	public IEnumerator<T> GetEnumerator() => _branches.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
