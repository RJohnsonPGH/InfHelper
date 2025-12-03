using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections;

public sealed record InfLeafSection<T> : IEnumerable<T> where T : IInfLeaf<T>
{
	internal InfLeafSection(string name, InfSectionCollection allSections, InfSectionExtCollection section)
	{
		_name = name;
		_leafs = section
			.SelectMany(extension => extension.Value
				.Select(entry => T.Create(entry.Key, extension.Key, entry.Value)));
	}

	public string Name => _name;
	private readonly string _name;

	private readonly IEnumerable<T> _leafs;
	public IEnumerator<T> GetEnumerator() => _leafs.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed record InfBranchSection<T> : IEnumerable<T> where T : IInfBranch<T>
{
	internal InfBranchSection(string name, InfSectionCollection allSections, InfSectionExtCollection entries)
	{
		_name = name;
		_branches = entries
			.SelectMany(extension => extension.Value
				.Select(entry => T.Create(entry.Key, extension.Key, entry.Value, allSections)));
	}
	public string Name => _name;
	private readonly string _name;
	private readonly IEnumerable<T> _branches;
	public IEnumerator<T> GetEnumerator() => _branches.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed record InfRootSection<T> : IEnumerable<T> where T : IInfRoot<T>
{
	internal InfRootSection(string name, InfSectionCollection allSections, InfSectionExtEntryCollection entries)
	{
		_name = name;
		_branches = entries
				.Select(entry => T.Create(entry.Key, entry.Value, allSections));
	}

	public string Name => _name;
	private readonly string _name;

	private readonly IEnumerable<T> _branches;
	public IEnumerator<T> GetEnumerator() => _branches.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
