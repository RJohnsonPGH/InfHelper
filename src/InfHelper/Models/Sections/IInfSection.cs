namespace InfHelper.Models.Sections;

public interface IInfLeaf<T> where T : IInfLeaf<T>
{
	internal static abstract T Create(string name, string extension, Entry entry);
}

public interface IInfBranch<T> where T : IInfBranch<T>
{
	internal static abstract T Create(string name, string extension, Entry entry, InfSectionCollection allSections);
}

public interface IInfRoot<T> where T : IInfRoot<T>
{
	internal static abstract T Create(string name, Entry entry, InfSectionCollection allSections);
}
