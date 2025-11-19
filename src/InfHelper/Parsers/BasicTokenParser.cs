using InfHelper.Exceptions;
using InfHelper.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfHelper.Parsers;

public class BasicTokenParser : ITokenParser
{
	public static ISet<TokenType> AllAvailableTokens => new HashSet<TokenType>
	{
		TokenType.CategoryClosing,
		TokenType.CategoryOpening,
		TokenType.Equality,
		TokenType.InlineComment,
		TokenType.NewLine,
		TokenType.Space,
		TokenType.WhiteSpace,
		TokenType.LineConcatenator,
		TokenType.Letter,
		TokenType.ValueSeparator,
		TokenType.ValueMarker
	};

	private ISet<TokenType> allTokens;

    public ISet<TokenType> AllTokens
    {
        get => allTokens;
        private set
        {
            //Sort by priority - some tokens share symbols e.g. line concentrator and letter
            allTokens = new HashSet<TokenType>(value.OrderByDescending(x => (int)x));
        }
    }

    public uint Length { get; private set; }
    public uint Position { get; private set; }

    public ISet<TokenType> AllowedTokens { get; set; }
    public ISet<TokenType> IgnoredTokens { get; set; }

    public event EventHandler<TokenEventArgs> InvalidTokenFound = default!;
    public event EventHandler<TokenEventArgs> ValidTokenFound = default!;

    public BasicTokenParser() : this(new HashSet<TokenType>(), new HashSet<TokenType>())
    {
        AllTokens = AllAvailableTokens;
    }

    public BasicTokenParser(ISet<TokenType> allowedTokens, ISet<TokenType> ignoredTokens)
    {
        allTokens = AllAvailableTokens;
		AllowedTokens = allowedTokens;
        IgnoredTokens = ignoredTokens;
    }

    public BasicTokenParser(ISet<TokenType> allTokens, ISet<TokenType> allowedTokens, ISet<TokenType> ignoredTokens)
    {
        this.allTokens = allTokens;
        AllowedTokens = allowedTokens;
        IgnoredTokens = ignoredTokens;
    }

    public virtual void Parse(string formula)
    {
        int row = 0, col = 0;
        string line = "";

        Length = (uint)formula.Length;
        Position = 0;

        foreach (var c in formula)
        {
            bool found = false;
            Position += 1;

            if (c == '\n')
            {
                row++;
                col = 0;
                line = "";
            }
            col++;
            line += c;

            //examine all known tokens
            foreach (var token in AllTokens)
            {

                if (!token.IsToken(c))
                {
                    continue;
                }

                //tokenBase found
                found = true;

                //ignored tokenBase detected
                if (IgnoredTokens != null && IgnoredTokens.Any(x => x == token))
                {
                    continue;
                }

                //not allowed tokenBase detected
                if (AllowedTokens == null || AllowedTokens.All(x => x != token))
                {
                    InvalidTokenFound?.Invoke(this, new(token, c));
                    continue;
                }

                //allowed tokenBase detected
                ValidTokenFound?.Invoke(this, new(token, c));
                break;
            }

            //tokenBase not recognized
            if (!found)
            {
                throw new NoneTokenRecognizedException($"None tokenBase recognized in row:{row} col:{col}{Environment.NewLine}" +
                    $"Examined symbol: {c}{Environment.NewLine}" +
                    $"Symbol number: {Convert.ToInt16(c)}{Environment.NewLine}" +
                    $"Examined line: {line}");
            }
        }
    }
}