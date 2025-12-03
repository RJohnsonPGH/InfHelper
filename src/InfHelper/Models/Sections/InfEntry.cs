namespace InfHelper.Models.Sections;

/// <summary>
/// Represents an entry with a specific section name, section extension, and associated entry data within an INF structure.
/// Leaf entries do not have any additional nested sections.
/// </summary>
/// <param name="Name">The section name that contains this entry. Cannot be null or empty.</param>
/// <param name="Extension">The section name extension that contains this entry. Cannot be null; may be empty if the section has no extension.</param>
/// <param name="Entry">The underlying entry data associated with this entry. Cannot be null.</param>
public abstract record InfLeafEntry(string Name, string Extension, Entry Entry);

/// <summary>
/// Represents an entry within an INF file branch, including its associated extension.
/// </summary>
/// <param name="Name">The section name that contains this entry. Cannot be null or empty.</param>
/// <param name="Extension">The section name extension that contains this entry. Cannot be null; may be empty if the section has no extension.</param>
/// <param name="Entry">The underlying entry data associated with this entry. Cannot be null.</param>
public abstract record InfBranchEntry(string Name, string Extension, Entry Entry);

/// <summary>
/// Represents an entry at the root level of an INF file structure.
/// </summary>
/// <param name="Name">The section name that contains this root entry. Cannot be null or empty.</param>
/// <param name="Entry">The entry associated with the root name. Cannot be null.</param>
public abstract record InfRootEntry(string Name, Entry Entry);
