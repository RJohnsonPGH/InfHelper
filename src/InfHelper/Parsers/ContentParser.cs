using InfHelper.Exceptions;
using InfHelper.Models;
using InfHelper.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfHelper.Parsers;

public class ContentParser
{
	private Category? currentCategory;
	private Key? currentKey;
	private readonly ITokenParser parser;
	private string? keyTmpValue;
	private Action? previousParsing;
	private string parsingType = "";

	/// <summary>
	/// When category parsing is completed
	/// </summary>
	public event EventHandler<Category>? CategoryDiscovered;

	public ContentParser() : this(new BasicTokenParser()) { }

	public ContentParser(ITokenParser parser)
	{
		this.parser = parser;
		parser.InvalidTokenFound += InvalidTokenFound;
	}

	public void Parse(string content)
	{
		InitMainParsing();
		parser.Parse(content);
		ValueParsingComplete();
		KeyParsingComplete();
		CategoryParsingComplete();
	}

	/// <summary>
	/// Inits main parsing state. Skips inlines comments, white spaces and new lines and init new category parsing when category opening tokenBase found.
	/// </summary>
	protected void InitMainParsing()
	{
		parsingType = "main parsing";
		ClearAllMyCallbacks();
		parser.ValidTokenFound += ValidTokenFoundDuringMainParsing;

		parser.AllowedTokens = new HashSet<TokenType>
		{
			TokenType.InlineComment,
			TokenType.CategoryOpening
		};

		parser.IgnoredTokens = new HashSet<TokenType>
		{
			TokenType.WhiteSpace,
			TokenType.NewLine
		};
	}

	/// <summary>
	/// Parse only letter tokens, end parsing when closing tokenBase found.
	/// </summary>
	protected void InitCategoryParsing()
	{
		parsingType = "category parsing";
		currentCategory = new Category();
		ClearAllMyCallbacks();
		parser.ValidTokenFound += ValidTokenFoundDuringCategoryParsing;
		parser.IgnoredTokens?.Clear();
		parser.AllowedTokens = new HashSet<TokenType>
		{
			TokenType.Space,
			TokenType.CategoryClosing,
			TokenType.Letter,
			TokenType.LineConcatenator
		};
	}

	/// <summary>
	/// Parse key id.
	/// </summary>
	protected void InitKeyIdParsing()
	{
		parsingType = "key id parsing";
		currentKey = new Key();
		ClearAllMyCallbacks();
		parser.ValidTokenFound += ValidTokenFoundDuringKeyIdParsing;

		parser.AllowedTokens = new HashSet<TokenType>
		{
			TokenType.InlineComment,
			TokenType.Letter,
			TokenType.Equality,
			TokenType.Space,
			TokenType.WhiteSpace,
			TokenType.CategoryOpening,
			TokenType.NewLine,
			TokenType.ValueSeparator,
			TokenType.ValueMarker
		};

		parser.IgnoredTokens = new HashSet<TokenType>
		{
			TokenType.LineConcatenator
		};
	}

	/// <summary>
	/// Parse values for current key
	/// </summary>
	protected void InitKeyValueParsing()
	{
		parsingType = "key value parsing";
		ClearAllMyCallbacks();
		parser.ValidTokenFound += ValidTokenFoundDuringKeyValueParsing;

		parser.AllowedTokens = new HashSet<TokenType>
		{
			TokenType.ValueSeparator,
			TokenType.Letter,
			TokenType.NewLine,
			TokenType.Space,
			TokenType.WhiteSpace,
			TokenType.InlineComment,
			TokenType.ValueMarker
		};

		parser.IgnoredTokens = new HashSet<TokenType>()
		{
			TokenType.LineConcatenator,
			TokenType.Equality
		};
	}

	protected void InitPureValueParsing()
	{
		parsingType = "pure value parsing";
		ClearAllMyCallbacks();
		parser.ValidTokenFound += ValidTokenFoundDuringPureValueParsing;

		parser.AllowedTokens = new HashSet<TokenType>
		{
			TokenType.Letter,
			TokenType.ValueMarker,
			TokenType.Space,
			TokenType.WhiteSpace,
			TokenType.ValueSeparator,
			TokenType.NewLine
		};

		parser.IgnoredTokens = new HashSet<TokenType>()
		{
			TokenType.InlineComment,
			TokenType.LineConcatenator,
			TokenType.Equality,
			TokenType.CategoryOpening,
			TokenType.CategoryClosing
		};
	}

	/// <summary>
	/// Parse comments
	/// </summary>
	protected void InitCommentParsing(Action previous)
	{
		parsingType = "comment parsing";
		previousParsing = previous;
		ClearAllMyCallbacks();
		parser.ValidTokenFound += ValidTokenFoundDuringCommentParsing;

		parser.AllowedTokens = new HashSet<TokenType>
		{
			TokenType.NewLine
		};

		parser.IgnoredTokens = new HashSet<TokenType>()
		{
			TokenType.Letter,
			TokenType.Space,
			TokenType.WhiteSpace,
			TokenType.ValueMarker,
			TokenType.ValueSeparator,
			TokenType.LineConcatenator,
			TokenType.Equality,
			TokenType.InlineComment,
			TokenType.CategoryOpening,
			TokenType.CategoryClosing
		};
	}

	// Parsing value inside ""
	private void ValidTokenFoundDuringPureValueParsing(object? _, TokenEventArgs eventArgs)
	{
		switch (eventArgs.TokenType)
		{
			case TokenType.Letter:
			case TokenType.ValueSeparator:
			case TokenType.WhiteSpace:
			case TokenType.Space:
				keyTmpValue += eventArgs.Symbol;
				break;
			case TokenType.ValueMarker:
				if (!string.IsNullOrEmpty(currentKey?.Id))
				{
					ValueParsingComplete(true);
					InitKeyValueParsing();
				}
				else
				{
					InitKeyIdParsing();
				}
				break;
			case TokenType.NewLine:
				ValueParsingComplete(true);
				KeyParsingComplete();
				InitKeyIdParsing();
				break;
			default:
				throw new InvalidTokenException("Invalid tokenBase found during comment parsing: " + eventArgs.Symbol);
		}
	}

	//Parsing inline comment
	private void ValidTokenFoundDuringCommentParsing(object? _, TokenEventArgs eventArgs)
	{
		switch (eventArgs.TokenType)
		{
			case TokenType.NewLine:
				if (previousParsing is null)
				{
					throw new InvalidOperationException("Previous parsing action is null when trying to return to it after comment parsing.");
				}
				previousParsing();
				break;
			default:
				throw new InvalidTokenException("Invalid tokenBase found during comment parsing: " + eventArgs.Symbol);
		}
	}

	//Parsing top layer
	protected void ValidTokenFoundDuringMainParsing(object? _, TokenEventArgs eventArgs)
	{
		switch (eventArgs.TokenType)
		{
			case TokenType.InlineComment:
				//go to next line, init comment parsing
				InitCommentParsing(InitMainParsing);
				break;
			case TokenType.CategoryOpening:
				currentCategory = new Category();
				InitCategoryParsing();
				break;
			default:
				throw new InvalidTokenException("Invalid tokenBase found during parsing of the file: " + eventArgs.Symbol);
		}
	}

	//when parsing a category
	protected void ValidTokenFoundDuringCategoryParsing(object? _, TokenEventArgs eventArgs)
	{
		switch (eventArgs.TokenType)
		{
			case TokenType.CategoryClosing:
				InitKeyIdParsing();
				break;
			case TokenType.LineConcatenator:
				if (this.parser.Position == (this.parser.Length - 1))
				{
					throw new InvalidTokenException(@"'\' are not allowed as the last token in a Category");
				}
				if (currentCategory is null)
				{
					throw new InvalidOperationException("Current category is null when trying to parse line concatenator.");
				}
				currentCategory.Name += eventArgs.Symbol;
				break;
			case TokenType.Letter:
			case TokenType.Space:
				if (currentCategory is null)
				{
					throw new InvalidOperationException("Current category is null when trying to parse line concatenator.");
				}
				currentCategory.Name += eventArgs.Symbol;
				break;
			default:
				throw new InvalidTokenException("Invalid tokenBase found during parsing of the file: " + eventArgs.Symbol);
		}
	}

	//when parsing a tokenBase id
	protected void ValidTokenFoundDuringKeyIdParsing(object? _, TokenEventArgs eventArgs)
	{
		switch (eventArgs.TokenType)
		{
			case TokenType.ValueMarker:
				InitPureValueParsing();
				break;
			case TokenType.ValueSeparator:
				SerializeCurrentTmpValueAsAnonymousKey();
				break;
			case TokenType.NewLine:
				if (!string.IsNullOrEmpty(keyTmpValue))
					SerializeCurrentTmpValueAsAnonymousKey();
				break;
			case TokenType.Equality:
				// multiple EQ tokens in formula
				if (!string.IsNullOrEmpty(currentKey?.Id))
				{
					throw new InvalidTokenException("Equality tokenBase detected, but not expected.");
				}
				KeyIdParsingCompleted();
				InitKeyValueParsing();
				break;
			case TokenType.WhiteSpace:
			case TokenType.Space:
				//ignore spaces at the begining
				if (!string.IsNullOrEmpty(keyTmpValue))
				{
					keyTmpValue += eventArgs.Symbol;
				}
				break;
			case TokenType.Letter:
				keyTmpValue += eventArgs.Symbol;
				break;
			case TokenType.CategoryOpening:
				KeyParsingComplete();
				CategoryParsingComplete();
				InitCategoryParsing();
				break;
			case TokenType.InlineComment:
				InitCommentParsing(InitKeyIdParsing);
				break;
			default:
				throw new InvalidTokenException("Invalid tokenBase found during parsing of the file: " + eventArgs.Symbol);
		}
	}

	//When parsing value
	protected void ValidTokenFoundDuringKeyValueParsing(object? _, TokenEventArgs eventArgs)
	{
		switch (eventArgs.TokenType)
		{
			case TokenType.ValueSeparator:
				ValueParsingComplete(allowNull: true);
				break;
			case TokenType.Letter:
				keyTmpValue += eventArgs.Symbol;
				break;
			case TokenType.NewLine:
				ValueParsingComplete();
				KeyParsingComplete();
				InitKeyIdParsing();
				break;
			case TokenType.WhiteSpace:
				if (string.IsNullOrEmpty(keyTmpValue))
				{
					ValueParsingComplete();
				}
				break;
			case TokenType.ValueMarker:
				InitPureValueParsing();
				break;
			case TokenType.InlineComment:
				ValueParsingComplete();
				KeyParsingComplete();
				InitCommentParsing(InitKeyIdParsing);
				break;
		}
	}

	protected void InvalidTokenFound(object? sender, TokenEventArgs eventArgs)
	{
		var builder = new StringBuilder();
		builder.AppendLine($"Invalid tokenBase found during {parsingType} parsing: ");
		builder.AppendLine($"Symbol: {eventArgs.Symbol}");
		builder.AppendLine($"Token type: {eventArgs.TokenType}");
		builder.AppendLine($"Allowed tokens: {string.Join(", ", parser.AllowedTokens.Select(t => $"{t}"))}");
		builder.AppendLine($"Ignored tokens: {string.Join(", ", parser.IgnoredTokens.Select(t => $"{t}"))}");
		throw new InvalidTokenException(builder.ToString());
	}

	protected void KeyParsingComplete()
	{
		if (currentKey != null && currentKey.KeyValues.Count != 0)
		{
			if (currentCategory is null)
			{
				throw new InvalidOperationException("Current category is null when trying to add key to it.");
			}

			currentCategory.Keys.Add(currentKey);
			currentKey = null;
		}
	}

	protected void ValueParsingComplete(bool pure = false, bool allowNull = false)
	{
		if (allowNull || !string.IsNullOrEmpty(keyTmpValue))
		{
			if (currentKey is null)
			{
				throw new InvalidOperationException("Current key is null when trying to add value to it.");
			}

			KeyValue keyValue;
			if (!pure)
			{
				keyValue = new KeyValue
				{
					Value = keyTmpValue
				};
			}
			else
			{
				keyValue = new PureValue
				{
					Value = keyTmpValue
				};
			}
			currentKey.KeyValues.Add(keyValue);
			keyTmpValue = null;
		}
	}

	protected void CategoryParsingComplete()
	{
		if (currentCategory != null)
		{
			CategoryDiscovered?.Invoke(this, currentCategory);
			currentCategory = null;
		}
	}

	protected void SerializeCurrentTmpValueAsAnonymousKey()
	{
		if (currentKey is null)
		{
			throw new InvalidOperationException("Current key is null when trying to add anonymous key.");
		}

		//TODO Implement this
		var keyValue = new KeyValue
		{
			Value = keyTmpValue
		};
		currentKey.KeyValues.Add(keyValue);
		keyTmpValue = null;
	}

	protected void KeyIdParsingCompleted()
	{
		if (currentKey is null ||
			keyTmpValue is null)
		{
			throw new InvalidOperationException("Current key or keyTmpValue is null when trying to complete key id parsing.");
		}

		// trim any leading or trailing whitespace
		currentKey.Id = keyTmpValue.Trim();
		keyTmpValue = null;
	}

	private void ClearAllMyCallbacks()
	{
		parser.ValidTokenFound -= ValidTokenFoundDuringMainParsing;
		parser.ValidTokenFound -= ValidTokenFoundDuringCategoryParsing;
		parser.ValidTokenFound -= ValidTokenFoundDuringKeyIdParsing;
		parser.ValidTokenFound -= ValidTokenFoundDuringKeyValueParsing;
		parser.ValidTokenFound -= ValidTokenFoundDuringCommentParsing;
		parser.ValidTokenFound -= ValidTokenFoundDuringPureValueParsing;
	}
}