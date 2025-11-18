using System;
using System.Collections.Generic;

namespace InfHelper.Models.Tokens;

/// <summary>
/// Represents a token that matches any non-control, non-whitespace Unicode letter character.
/// </summary>
public class LetterToken : TokenBase
{
    public override char[] Symbols => _symbols.Value;

	// Only compute the symbols once, lazily.
	private readonly Lazy<char[]> _symbols = new(() =>
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

    public override TokenType Type { get; } = TokenType.Letter;

    public override bool IsToken(char c)
    {
        return !char.IsControl(c) && !char.IsWhiteSpace(c);
    }
}