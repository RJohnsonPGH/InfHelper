using InfHelper.Exceptions;
using InfHelper.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

	private ISet<TokenType> _allTokenTypes;

	public ISet<TokenType> AllTokenTypes
	{
		get => _allTokenTypes;
		private set
		{
			//Sort by priority - some tokens share symbols e.g. line concentrator and letter
			_allTokenTypes = new HashSet<TokenType>(value.OrderByDescending(x => (int)x));
		}
	}

	public uint Length { get; private set; }
	public uint Position { get; private set; }

	public ISet<TokenType> AllowedTokens { get; set; }
	public ISet<TokenType> IgnoredTokens { get; set; }

	public event EventHandler<TokenEventArgs> ValidTokenFound = default!;

	public BasicTokenParser() : this(new HashSet<TokenType>(), new HashSet<TokenType>())
	{
		AllTokenTypes = AllAvailableTokens;
	}

	public BasicTokenParser(ISet<TokenType> allowedTokens, ISet<TokenType> ignoredTokens)
	{
		_allTokenTypes = AllAvailableTokens;
		AllowedTokens = allowedTokens;
		IgnoredTokens = ignoredTokens;
	}

	public BasicTokenParser(ISet<TokenType> allTokens, ISet<TokenType> allowedTokens, ISet<TokenType> ignoredTokens)
	{
		this._allTokenTypes = allTokens;
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
			foreach (var tokenType in AllTokenTypes)
			{
				if (!tokenType.IsToken(c))
				{
					continue;
				}

				//tokenBase found
				found = true;

				//ignored tokenBase detected
				if (IgnoredTokens != null && IgnoredTokens.Any(x => x == tokenType))
				{
					continue;
				}

				//not allowed tokenBase detected
				if (AllowedTokens == null || AllowedTokens.All(x => x != tokenType))
				{
					var builder = new StringBuilder();
					//builder.AppendLine($"Invalid tokenBase found during {_parsingType} parsing: ");
					builder.AppendLine($"Row: {row}, Column: {col}");
					builder.AppendLine($"Symbol: {c} ({(int)c})");
					builder.AppendLine($"Token type: {tokenType}");
					builder.AppendLine($"Allowed tokens: {string.Join(", ", AllowedTokens?.Select(t => $"{t}") ?? [])}");
					builder.AppendLine($"Ignored tokens: {string.Join(", ", IgnoredTokens?.Select(t => $"{t}") ?? [])}");
					throw new InvalidTokenException(builder.ToString());
				}

				//allowed tokenBase detected
				ValidTokenFound?.Invoke(this, new(tokenType, c));
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