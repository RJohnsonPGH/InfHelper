using InfHelper.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Sections;

public sealed record InfModel : InfBranchEntry, IInfBranch<InfModel>
{
	internal InfModel(string name, string extension, Entry entry, InfSectionCollection allSections) : base(name, extension, entry)
	{
		_install = new(() => TypedInfData.ParseBranchSection<InfInstall>(allSections, entry.Values[0].Value));
		DeviceIds = entry
			.Values
			.Skip(1)
			.Select(x => x.Value);
	}

	public InfBranchSection<InfInstall> Install => _install.Value;
	private Lazy<InfBranchSection<InfInstall>> _install;
	public IEnumerable<string> DeviceIds { get; }

	static InfModel IInfBranch<InfModel>.Create(string name, string extension, Entry entry, InfSectionCollection allSections)
	{
		// Models must have at least two entries: the install section, and the model ID
		if (entry.Values.Count < 2)
		{
			throw new RequiredEntryMalformedException(extension, name, entry);
		}
		return new(name, extension, entry, allSections);
	}
}