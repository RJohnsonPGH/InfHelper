using InfHelper.Exceptions;
using InfHelper.Models.Tokens;
using InfHelper.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace InfHelperTests.Parsers;

[TestClass()]
public class BasicTokenParserTests
{
    [TestMethod()]
    public void TokenOrderTest()
    {
        string formula = "[TEST]";
        var parser = new BasicTokenParser(allowedTokens: new HashSet<TokenType>
        {
            TokenType.CategoryOpening,
            TokenType.Letter,
            TokenType.CategoryClosing
        }, ignoredTokens: new HashSet<TokenType>());
        string result = "";
        parser.ValidTokenFound += (sender, token) => result += token.Symbol;
        parser.Parse(formula);

        Assert.AreEqual(formula, result);
    }

    [TestMethod()]
    public void IgnoredTokensTest()
    {
        string expression = "[TEST]";
        string formula = "  \n   " + expression + "   \n   ";
        var parser = new BasicTokenParser(allowedTokens: new HashSet<TokenType>
        {
            TokenType.CategoryOpening,
            TokenType.Letter,
            TokenType.CategoryClosing
        }, ignoredTokens: new HashSet<TokenType>
        {
            TokenType.WhiteSpace,
            TokenType.NewLine
        });

        string result = "";
        parser.ValidTokenFound += (sender, token) => result += token.Symbol;
        parser.Parse(formula);

        Assert.AreEqual(expression, result);
    }

    [TestMethod()]
    public void AllowedTokensTest()
    {
        string formula = "[TE;ST] \\";
        var parser = new BasicTokenParser(allowedTokens: new HashSet<TokenType>
        {
            TokenType.CategoryOpening,
            TokenType.Letter,
            TokenType.CategoryClosing
        }, ignoredTokens: new HashSet<TokenType>
        {
            TokenType.WhiteSpace,
            TokenType.NewLine
        });

        string result = "";
        //string invalids = "";
        parser.ValidTokenFound += (sender, token) => result += token.Symbol;

        Assert.ThrowsException<InvalidTokenException>(() => parser.Parse(formula));
        //Assert.IsTrue(invalids.Contains(';') && invalids.Contains('\\'));
    }

    [TestMethod()]
    public void TokensWithSameSymbol()
    {
        string formula = "Test = test\\\ntest";
        var parser = new BasicTokenParser(allowedTokens: new HashSet<TokenType>
        {
            TokenType.Letter,
            TokenType.Equality,
            TokenType.LineConcatenator
        }, ignoredTokens: new HashSet<TokenType>
        {
            TokenType.WhiteSpace,
            TokenType.NewLine
        });

        var tokens = new List<TokenType>();
        parser.ValidTokenFound += (sender, tokenEventArgs) => tokens.Add(tokenEventArgs.TokenType);
        parser.Parse(formula);

        Assert.IsTrue(tokens.Contains(TokenType.Equality) && tokens.Contains(TokenType.Letter) && tokens.Contains(TokenType.LineConcatenator));
    }

    [TestMethod()]
    public void TestOfAdaptability()
    {
        string formula = "Test = test\\\ntest";
        var parser = new BasicTokenParser(allowedTokens: new HashSet<TokenType>
        {
            TokenType.Letter,
            TokenType.Equality
        }, ignoredTokens: new HashSet<TokenType>
        {
            TokenType.WhiteSpace
        });

        string id = "";
        string key = "";

        void keyParsing(object sender, TokenEventArgs eventArgs)
        {
            switch (eventArgs.TokenType)
            {
                case TokenType.Letter:
                    id += eventArgs.Symbol;
                    break;
                case TokenType.Equality:
                    parser.ValidTokenFound -= keyParsing;
                    parser.AllowedTokens = new HashSet<TokenType>()
                    {
                        TokenType.Letter,
                        TokenType.NewLine,
                        TokenType.LineConcatenator
                    };
                    parser.ValidTokenFound += valueParsing;
                    break;
            }
        }

        void valueParsing(object sender, TokenEventArgs eventArgs)
        {
            switch (eventArgs.TokenType)
            {
                case TokenType.Letter:
                    key += eventArgs.Symbol;
                    break;
                case TokenType.LineConcatenator:
                    parser.IgnoredTokens.Add(TokenType.NewLine);                  
                    break;
            }
        }

        parser.ValidTokenFound += keyParsing;
        parser.Parse(formula);

        Assert.AreEqual(id,"Test");
        Assert.AreEqual(key,"testtest");
    }
}