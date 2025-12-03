using System;
using System.Collections;
using System.Collections.Generic;

namespace InfHelper.Models;

public class Section : IEnumerable<Entry>
{
	/// <summary>
	/// Name of the category - retrieved from brackets [CategoryName]
	/// </summary>
	public string Name { get; set; } = string.Empty;

	public List<Entry> Entries => _entries;
	private readonly List<Entry> _entries = [];

	/// <summary>
	/// Check if the Category is named like the given parameter - not case sensitive
	/// </summary>
	/// <param name="name">Compare to given name</param>
	/// <returns></returns>
	public bool IsNamed(string name) => string.Compare(Name, name, StringComparison.OrdinalIgnoreCase) == 0;

	public bool IsBaseSection(string name) => string.Compare(Name.Split('.')[0], name, StringComparison.OrdinalIgnoreCase) == 0;

	public Entry this[int index]
	{
		get => Entries[index];
		set => Entries[index] = value;
	}

	public IEnumerator<Entry> GetEnumerator() => _entries.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();
}