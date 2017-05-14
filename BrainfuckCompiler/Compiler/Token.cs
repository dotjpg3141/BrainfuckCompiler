namespace BrainfuckCompiler.Compiler
{
    public class Token
    {
        public Token(TokenType type, string value, CursorPosition start, CursorPosition end)
        {
            this.Type = type;
            this.Value = value;
            this.Start = start;
            this.End = end;
        }

        public TokenType Type { get; }

        public string Value { get; }

        public CursorPosition Start { get; }

        public CursorPosition End { get; }

        public Token WithType(TokenType type)
        {
            return new Token(type, this.Value, this.Start, this.End);
        }
    }
}