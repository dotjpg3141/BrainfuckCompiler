using System;
using System.Collections.Generic;
using System.Text;

namespace BrainfuckCompiler.Compiler
{
    public class Tokenizer : ObjectStream<Token>
    {
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>()
        {
            ["func"] = TokenType.Func,
            ["var"] = TokenType.Var,
            ["return"] = TokenType.Return,
            ["while"] = TokenType.While,
            ["do"] = TokenType.Do,
            ["if"] = TokenType.If,
            ["else"] = TokenType.Else,
        };

        private CursorPosition start;

        public Tokenizer(CharReader reader)
        {
            this.Reader = reader;
        }

        public CharReader Reader { get; }

        protected override bool OnMoveNext(out Token current)
        {
            // skip whitespace & set start position
            this.start = this.Reader.Cursor;
            while (this.Reader.MoveNext() && IsWhitespace(this.Reader.Current))
            {
                this.start = this.Reader.Cursor;
            }

            // handle eof
            if (this.Reader.EndOfStream)
            {
                current = null;
                return false;
            }

            if (IsAlpha(this.Reader.Current))
            {
                // identifier
                current = this.ParseIdentifier();

                // keyword
                if (Keywords.ContainsKey(current.Value))
                {
                    current = current.WithType(Keywords[current.Value]);
                }
            }
            else if (IsNum(this.Reader.Current))
            {
                // number
                current = this.ParseNumber();
            }
            else if (this.Reader.Current == '\'')
            {
                current = this.ParseChar();
            }
            else
            {
                switch (this.Reader.Current)
                {
                    case '=':
                        current = this.Matches('=')
                            ? this.NewToken(TokenType.Equal, "==")
                            : this.NewToken(TokenType.Assign, "=");
                        break;
                    case '>': current = this.NewTokenChar(TokenType.Greater); break;
                    case '+': current = this.NewTokenChar(TokenType.Add); break;
                    case '-': current = this.NewTokenChar(TokenType.Sub); break;
                    case '*': current = this.NewTokenChar(TokenType.Mul); break;
                    case '/': current = this.NewTokenChar(TokenType.Div); break;
                    case '(': current = this.NewTokenChar(TokenType.ParenthesisBegin); break;
                    case ')': current = this.NewTokenChar(TokenType.ParenthesisEnd); break;
                    case ',': current = this.NewTokenChar(TokenType.ArgumentSeperator); break;
                    case ':': current = this.NewTokenChar(TokenType.TypeSeperator); break;
                    case '{': current = this.NewTokenChar(TokenType.BracketBegin); break;
                    case '}': current = this.NewTokenChar(TokenType.BracketEnd); break;
                    case ';': current = this.NewTokenChar(TokenType.StatementSeperator); break;
                    default:
                        throw new Exception($"unexpected symbol '{this.Reader.Current}' at {this.Reader.Cursor}");
                }
            }

            return true;
        }

        private Token ParseIdentifier()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Reader.Current);

            while (this.Reader.CanPeek() && IsAlphaNum(this.Reader.Peek()))
            {
                this.Reader.MoveNext();
                sb.Append(this.Reader.Current);
            }

            return this.NewToken(TokenType.Identifier, sb.ToString());
        }

        private Token ParseNumber()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Reader.Current);

            while (this.Reader.CanPeek() && IsNum(this.Reader.Peek()))
            {
                this.Reader.MoveNext();
                sb.Append(this.Reader.Current);
            }

            return this.NewToken(TokenType.Number, sb.ToString());
        }

        private Token ParseChar()
        {
            this.Reader.MoveNext();
            char result;
            if (this.Reader.Current != '\\')
            {
                result = this.Reader.Current;
            }
            else
            {
                this.Reader.MoveNext();
                switch (this.Reader.Current)
                {
                    case 'r':
                        result = '\r';
                        break;
                    case 'n':
                        result = '\n';
                        break;
                    case 't':
                        result = '\t';
                        break;
                    case '\\':
                        result = '\\';
                        break;
                    default:
                        throw new Exception($"invalid escape charater '{this.Reader.Current}'");
                }
            }

            this.Reader.MoveNext();
            if (this.Reader.Current != '\'')
            {
                throw new Exception($@"invalid symbol ""{this.Reader.Current}"". expected ""'\""");
            }

            return this.NewToken(TokenType.Char, result.ToString());
        }

        private Token NewTokenChar(TokenType type)
        {
            return this.NewToken(type, this.Reader.Current.ToString());
        }

        private Token NewToken(TokenType type, string value)
        {
            return this.NewToken(type, value, this.Reader.Cursor);
        }

        private Token NewToken(TokenType type, string value, CursorPosition end)
        {
            if (value == string.Empty)
            {
                throw new InvalidOperationException();
            }

            return new Token(type, value, this.start, end);
        }

        private bool Matches(char chr)
        {
            if (this.Reader.CanPeek() && this.Reader.Peek() == chr)
            {
                this.Reader.MoveNext();
                return true;
            }

            return false;
        }

        private static bool IsWhitespace(char chr) =>
            char.IsWhiteSpace(chr);

        private static bool IsAlpha(char chr) =>
            ('a' <= chr && chr <= 'z') || ('A' <= chr && chr <= 'Z') || chr == '_';

        private static bool IsNum(char chr) =>
            '0' <= chr && chr <= '9';

        private static bool IsAlphaNum(char chr) =>
            IsAlpha(chr) || IsNum(chr);
    }
}
