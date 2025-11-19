using InfHelper.Models.Tokens;
using System;
using System.Collections.Generic;

namespace InfHelper.Parsers;

public interface ITokenParser
{
    uint Length { get; }
    uint Position { get; }
    ISet<TokenType> AllowedTokens { get; set; }
    ISet<TokenType> AllTokens { get; }
    ISet<TokenType> IgnoredTokens { get; set; }
    event EventHandler<TokenEventArgs> InvalidTokenFound;
    event EventHandler<TokenEventArgs> ValidTokenFound;

    void Parse(string formula);
}