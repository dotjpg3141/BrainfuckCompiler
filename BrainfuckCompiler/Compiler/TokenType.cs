namespace BrainfuckCompiler.Compiler
{
    public enum TokenType
    {
        // Literals
        Number,
        Char,

        // Identifier & Keywords
        Identifier,
        Func,
        Var,
        Return,
        While,
        Do,
        If,
        Else,

        // Operators
        // TODO additional operators
        Assign, // =
        Equal,  // ==
        Add,    // +
        Sub,    // -
        Mul,    // *
        Div,    // /

        ParenthesisBegin,
        ParenthesisEnd,
        ArgumentSeperator,
        TypeSeperator,

        BracketBegin,
        BracketEnd,
        StatementSeperator,
    }
}
