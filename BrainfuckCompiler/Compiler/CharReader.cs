using System.IO;

namespace BrainfuckCompiler.Compiler
{
    public class CharReader : ObjectStream<char>
    {
        public CharReader(TextReader reader)
        {
            this.Reader = reader;
        }

        public TextReader Reader { get; }

        public int CurrentRow { get; private set; } = 1;

        public int CurrentColumn { get; private set; } = 1;

        public int CurrentIndex { get; private set; } = 0;

        public CursorPosition Cursor => new CursorPosition()
        {
            Column = this.CurrentColumn,
            Row = this.CurrentRow,
            Index = this.CurrentIndex
        };

        protected override bool OnMoveNext(out char current)
        {
            var nextChar = this.Reader.Read();

            // update cursor position: (\r\n), (\n), (\r)
            if (nextChar == '\r' || (this.Current != '\r' && nextChar == '\n'))
            {
                this.CurrentRow++;
            }

            if (nextChar == '\r' || nextChar == '\n')
            {
                this.CurrentColumn = 1;
            }
            else
            {
                this.CurrentColumn++;
            }

            this.CurrentIndex++;
            current = nextChar == -1 ? '\0' : (char)nextChar;
            return nextChar != -1;
        }
    }
}
