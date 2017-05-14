namespace BrainfuckCompiler.Compiler
{
    public struct CursorPosition
    {
        public static readonly CursorPosition Invalid = new CursorPosition(-1, -1, -1);

        public CursorPosition(int row, int col, int index)
            : this()
        {
            this.Row = row;
            this.Column = col;
            this.Index = index;
        }

        public int Row { get; set; }

        public int Column { get; set; }

        public int Index { get; set; }

        public override string ToString()
        {
            return $"Row={this.Row}, Column={this.Column}";
        }
    }
}