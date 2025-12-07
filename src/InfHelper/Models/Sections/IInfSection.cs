namespace InfHelper.Models.Sections;

/// <summary>
/// Defines a contract for a section within an INF file, supporting creation and initialization of section instances
/// with associated configuration data.
/// </summary>
/// <remarks>Implementations of this interface represent logical sections in an INF file and provide a
/// standardized way to create section instances using configuration entries and contextual information from other
/// sections. This interface is typically used in scenarios involving parsing, manipulation, or validation of INF file
/// structures.</remarks>
/// <typeparam name="T">The type that implements the section interface. Must implement <see cref="IInfSection{T}"/>.</typeparam>
public interface IInfSection<T> where T : IInfSection<T>
{
	/// <summary>
	/// Creates an instance of type T using the specified section name, entry, and collection of sections.
	/// </summary>
	/// <param name="sectionName">The name of the section to associate with the created instance. Cannot be null or empty.</param>
	/// <param name="entry">The entry containing configuration or data used to initialize the instance. Cannot be null.</param>
	/// <param name="allSections">A collection of all available sections, which may be used for cross-referencing or additional context during
	/// creation. Cannot be null.</param>
	/// <returns>An instance of type T initialized with the provided section name, entry, and section collection.</returns>
	internal static abstract T Create(string sectionName, Entry entry, InfSectionCollection allSections);
}
