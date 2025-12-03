namespace InfHelper.Models.Sections;

public sealed record InfStrings : InfLeafEntry, IInfLeaf<InfStrings>
{
	internal InfStrings(string name, string extension, Entry entry) : base(name, extension, entry) { }

	static InfStrings IInfLeaf<InfStrings>.Create(string name, string extension, Entry entry) =>
		new(name, extension, entry);
}
