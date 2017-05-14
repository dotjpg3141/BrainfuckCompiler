using System;

namespace BrainfuckCompiler.Compiler
{
    [Serializable]
    public class CompilerException : Exception
    {
        public CompilerException(Token start, Token end, string message)
            : base(message)
        {
            this.Start = start;
            this.End = end;
        }

        public CompilerException(Token token, string message)
            : this(token, token, message)
        {
        }

        public Token Start { get; }

        public Token End { get; }
    }
}
