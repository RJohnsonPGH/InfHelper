using System;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Models.Tokens;

/// <summary>
/// Specifies the types of tokens that can be identified during parsing or lexical analysis.
/// </summary>
/// <remarks>The order of the enum values is important, as the int value is used to handle symbol priority.</remarks>
public enum TokenType
{
    Letter,
    Equality,
    CategoryOpening,
	CategoryClosing,
    WhiteSpace,
	NewLine,
	LineConcatenator,
	ValueSeparator,
	ValueMarker,
	InlineComment,
	Space,
}

public static class TokenTypeExtensions
{
	private readonly static Lazy<HashSet<char>> _letterTokenTypeSymbols = new(() =>
	{
		var symbols = new HashSet<char>();
		for (int i = char.MinValue; i <= char.MaxValue; i++)
		{
			char c = Convert.ToChar(i);
			if (!char.IsControl(c) && !char.IsWhiteSpace(c))
			{
				symbols.Add(c);
			}
		}
		return [.. symbols];
	});

	// Static dictionary for only one allocation, HashSet for O(1) lookup time
	private static readonly Dictionary<TokenType, HashSet<char>> _symbols = Enum.GetValues<TokenType>().ToDictionary(
		t => t,
		t => t switch
		{
			TokenType.Letter => _letterTokenTypeSymbols.Value,
			TokenType.Equality => ['='],
			TokenType.CategoryOpening => ['['],
			TokenType.CategoryClosing => [']'],
			TokenType.WhiteSpace => [' ', '\t', '\u00a0'], // Non breaking space - do some INF files use this? Note, cannot use IsWhiteSpace as it includes newline characters.
			TokenType.NewLine => ['\n', '\r'],
			TokenType.LineConcatenator => ['\\'],
			TokenType.ValueSeparator => [','],
			TokenType.ValueMarker => ['"'],
			TokenType.InlineComment => [';'],
			TokenType.Space => [' '],
			_ => throw new InvalidOperationException("The specified TokenType is not recognized.")
		});


	public static HashSet<char> GetSymbols(this TokenType type) => _symbols[type];

	public static bool IsToken(this TokenType type, char c)
	{
		return type switch
		{
			TokenType.Letter => !char.IsControl(c) && !char.IsWhiteSpace(c),
			TokenType.WhiteSpace => char.IsWhiteSpace(c), // This is less restrictive than the predefined set (includes newline characters). Why?
			_ => type.GetSymbols().Contains(c),
		};
	}
}