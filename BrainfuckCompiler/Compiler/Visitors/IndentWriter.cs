using System;
using System.IO;

namespace BrainfuckCompiler.Compiler.Visitors
{
    public class IndentWriter
    {
        private TextWriter writer;
        private string indentString = string.Empty;
        private bool needIndent = true;

        public IndentWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        public void WriteLine()
        {
            this.WriteNewLine();
        }

        public void WriteLine(string line)
        {
            this.Write(line);
            this.WriteNewLine();
        }

        public void Write(string value)
        {
            value = value.Replace("\r\n", "\n").Replace('\r', '\n');

            int index = 0;
            while (true)
            {
                int start = index;
                int end = value.IndexOf('\n', start);

                if (this.needIndent)
                {
                    this.WriteIndent();
                }

                if (end == -1)
                {
                    this.writer.Write(value.Substring(start));
                    break;
                }
                else
                {
                    this.writer.Write(value.Substring(start, end));
                    this.WriteNewLine();
                }

                index = end + 1;
            }
        }

        public void Write(object value)
        {
            this.Write(value == null ? "null" : value.ToString());
        }

        public void WriteLine(object value)
        {
            this.WriteLine(value == null ? "null" : value.ToString());
        }

        public void Indent()
        {
            this.indentString += "\t";
        }

        public void Unindent()
        {
            if (this.indentString.Length == 0)
            {
                throw new InvalidOperationException("indentation underflow");
            }

            this.indentString = this.indentString.Substring(1);
        }

        private void WriteNewLine()
        {
            this.needIndent = true;
            this.writer.Write("\n");
        }

        private void WriteIndent()
        {
            this.writer.Write(this.indentString);
            this.needIndent = false;
        }
    }
}
