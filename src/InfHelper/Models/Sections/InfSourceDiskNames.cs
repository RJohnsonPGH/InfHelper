namespace InfHelper.Models.Sections;

//public sealed record InfSourceDiskNames : InfSection, IInfTopLevelSection<InfSourceDiskNames>
//{
//    private InfSourceDiskNames(string name, InfSectionCollection allSections, InfSectionExtCollection section) : base(section)
//	{
//		_name = name;
//        _allSections = allSections;
//	}

//	public string Name => _name;
//	private readonly string _name;
//    private readonly InfSectionCollection _allSections;

//	static string IInfTopLevelSection<InfSourceDiskNames>.SectionName => "SourceDisksNames";
//    static InfSourceDiskNames IInfSection<InfSourceDiskNames>.Create(string name, InfSectionCollection allSections, InfSectionExtCollection section) =>
//		new(name, allSections, section);
//}
