using System;
using System.Collections.Generic;

namespace InfHelper.Models.Sections;

public sealed record InfManufacturer : InfRootEntry, IInfRoot<InfManufacturer>
{
	internal InfManufacturer(string name, Entry entry, InfSectionCollection allSections) : base(name, entry)
	{
		_modelSection = new(TypedInfData.ParseBranchSection<InfModel>(allSections, entry.Values[0].Value));
	}

	public IEnumerable<InfModel> Models => _modelSection.Value;
	private readonly Lazy<InfBranchSection<InfModel>> _modelSection;

	static InfManufacturer IInfRoot<InfManufacturer>.Create(string name, Entry entry, InfSectionCollection allSections) =>
		new(name, entry, allSections);
}
