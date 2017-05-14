namespace BrainfuckCompiler.Compiler.Model
{
    public class Variable
    {
        public Token NameToken { get; set; }

        public Token TypeToken { get; set; }

        public string Name
        {
            get
            {
                return this.NameToken?.Value;
            }

            set
            {
                this.NameToken = new Token(TokenType.Identifier, value, CursorPosition.Invalid, CursorPosition.Invalid);
            }
        }

        public DataType Type { get; set; }
    }
}
