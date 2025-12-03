namespace InfHelper.Models.Sections;

public sealed record InfInstall : InfBranchEntry, IInfBranch<InfInstall>
{
	internal InfInstall(string name, string extension, Entry entry, InfSectionCollection allSections) : base(name, extension, entry)
	{
		// Parse directives
	}

	static InfInstall IInfBranch<InfInstall>.Create(string name, string extension, Entry entry, InfSectionCollection allSections) =>
		new(name, extension, entry, allSections);
}