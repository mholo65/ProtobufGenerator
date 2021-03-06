﻿using FluentAssertions;
using ProtobufCompiler.Compiler;
using ProtobufCompiler.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ProtobufCompiler.Tests
{
    public class LexerTests
    {
        private readonly ILexicalAnalyzer _sys;

        private readonly string _data = "syntax = \"proto3\";" + Environment.NewLine +
                                            "message Outer {" + Environment.NewLine +
                                               "/*This should start a block" + Environment.NewLine +
                                               "and this should end it.*/" + Environment.NewLine +
                                               "//This should start a line comment" + Environment.NewLine +
                                               "option my_option = true; // some comment" + Environment.NewLine +
                                               "message Inner {" + Environment.NewLine +
                                               "int64 ival = 1;" + Environment.NewLine +
                                               "}" + Environment.NewLine +
                                               "map<int32, string> my_map = 2;" + Environment.NewLine +
                                               "}";

        public LexerTests()
        {
            var memStream = new MemoryStream(Encoding.UTF8.GetBytes(_data));
            var reader = new StreamReader(memStream);
            var source = new Source(reader);
            _sys = new LexicalAnalyzer(source);
        }

        [Fact]
        public void ShouldTokenize64Tokens()
        {
            _sys.Tokenize();
            var tokens = _sys.TokenStream;
            tokens.Count.Should().Be(64, "because there are 64 tokens in the message definition, including EOLs");
        }

        [Fact]
        public void ShouldReturnTokenStream()
        {
            _sys.Tokenize();
            var tokenStream = _sys.TokenStream;
            tokenStream.Peek().Should().Be(new Token(TokenType.Id, 1, 1, "syntax"));
        }

        [Fact]
        public void ThereAreTenEndlineTokens()
        {
            _sys.Tokenize();
            var tokenStream = _sys.TokenStream.Where(t => t.Type == TokenType.EndLine);
            tokenStream.Count().Should().Be(10, "because there are 11 lines in the data.");
        }

        [Fact]
        public void ThereAreFiveIdTokens()
        {
            _sys.Tokenize();
            var tokenStream = _sys.TokenStream.Where(t => t.Type == TokenType.Id);
            tokenStream.Count().Should().Be(5, "because there are 5 types in the data : syntax, message, option, message, and map.");
        }

        [Fact]
        public void ThereAreFourCommentTokens()
        {
            _sys.Tokenize();
            var tokenStream = _sys.TokenStream.Where(t => t.Type == TokenType.Comment);
            tokenStream.Count().Should().Be(4, "because there are 4 different opening or closing comment tokens");
        }
    }
}