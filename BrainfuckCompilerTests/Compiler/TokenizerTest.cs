using System;
using System.Collections.Generic;
using System.IO;
using BrainfuckCompiler.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrainfuckCompilerTests.Compiler
{
    [TestClass]
    public class TokenizerTest
    {
        [TestMethod]
        public void Tokenizer_Empty()
        {
            var tokenizer = NewTokenizer(string.Empty);

            Assert.IsFalse(tokenizer.MoveNext());
            Assert.IsTrue(tokenizer.EndOfStream);
        }

        [TestMethod]
        public void Tokenizer_NumberIdentifier()
        {
            var number = "12345";
            var identifier = "test77";
            var tokenizer = NewTokenizer($"{number}{identifier}");

            Assert.IsTrue(tokenizer.MoveNext());
            AssertToken(tokenizer.Current, number, TokenType.Number);

            Assert.IsTrue(tokenizer.MoveNext());
            AssertToken(tokenizer.Current, identifier, TokenType.Identifier);

            Assert.IsFalse(tokenizer.MoveNext());
            Assert.IsTrue(tokenizer.EndOfStream);
        }

        [TestMethod]
        public void Tokenizer_Whitespace()
        {
            var tokenizer = NewTokenizer("\r\n\t ");

            Assert.IsFalse(tokenizer.MoveNext());
            Assert.IsTrue(tokenizer.EndOfStream);
        }

        [TestMethod]
        public void Tokenizer_SingleToken()
        {
            var tokenResults = new Dictionary<string, TokenType>()
            {
                ["hel_lo123"] = TokenType.Identifier,
                ["var"] = TokenType.Var,
                ["123456"] = TokenType.Number,
                ["+"] = TokenType.Add,
                ["-"] = TokenType.Sub,
                ["("] = TokenType.ParenthesisBegin,
                [")"] = TokenType.ParenthesisEnd,
            };

            foreach (var item in tokenResults)
            {
                var source = item.Key;
                TestSingleToken(source, (token) =>
                {
                    var msg = "; " + token.Value;
                    AssertToken(token, source, item.Value);
                    AssertCursorPosition(token.Start, 1, 1, 0, msg);

                    // AssertCursorPosition(token.End, 1, source.Length + 2, source.Length + 1, msg);
                });
            }
        }

        [TestMethod]
        public void Tokenizer_Char()
        {
            var tokenizer = NewTokenizer("'a'");
            Assert.IsTrue(tokenizer.MoveNext());
            AssertToken(tokenizer.Current, "a", TokenType.Char);

            tokenizer = NewTokenizer("'\\r'");
            Assert.IsTrue(tokenizer.MoveNext());
            AssertToken(tokenizer.Current, "\r", TokenType.Char);
        }

        private static Tokenizer NewTokenizer(string value)
        {
            return new Tokenizer(new CharReader(new StringReader(value)));
        }

        private static void AssertToken(Token token, string value, TokenType type)
        {
            Assert.IsNotNull(token);
            Assert.AreEqual(value, token.Value, "token value for " + token.Value);
            Assert.AreEqual(type, token.Type, "token type for " + token.Value);
        }

        private static void AssertCursorPosition(CursorPosition cp, int row, int col, int index, string msg = "")
        {
            Assert.AreEqual(row, cp.Row, "row" + msg);
            Assert.AreEqual(col, cp.Column, "col" + msg);
            Assert.AreEqual(index, cp.Index, "index" + msg);
        }

        private static void TestSingleToken(string source, Action<Token> test)
        {
            var tokenizer = NewTokenizer(source);

            Assert.IsTrue(tokenizer.MoveNext());
            Assert.IsFalse(tokenizer.EndOfStream);
            test(tokenizer.Current);

            Assert.IsFalse(tokenizer.MoveNext());
            Assert.IsTrue(tokenizer.EndOfStream);
        }
    }
}
