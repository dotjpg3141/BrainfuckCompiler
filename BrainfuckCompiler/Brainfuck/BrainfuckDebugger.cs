using System;
using System.Text.RegularExpressions;

namespace BrainfuckCompiler.Brainfuck
{
    public class BrainfuckDebugger : BrainfuckInterpreter
    {
        private const string Insns = @"[\+\-\<\>\.\,\[\]]";
        private readonly Regex regex = new Regex($"(?<!{Insns})(?={Insns})|(?<={Insns})(?!{Insns})");

        public BrainfuckDebugger(string source, byte[] cache)
            : base(source, cache)
        {
        }

        public void DebugDump()
        {
            Console.Clear();
            this.DumpSource();
            this.DumpCache();
        }

        private void DumpCache()
        {
            int indexLen = this.Cache.Length.ToString().Length;
            int columnCount = (Console.WindowWidth - indexLen - 6) / 5;
            for (int i = 0; i < this.Cache.Length; i += columnCount)
            {
                Console.Write(i.ToString().PadLeft(indexLen, '0'));
                Console.Write(" |");
                this.PrintRow(i, i + columnCount, 4, b => b.ToString());
                Console.Write(" | ");
                this.PrintRow(i, i + columnCount, 1, b => b < 32 ? "." : ((char)b).ToString());
                Console.WriteLine();
            }
        }

        private void DumpSource()
        {
            var sourceItems = this.regex.Split(this.Source);
            int index = 0;
            foreach (var item in sourceItems)
            {
                InvertConsoleColor();
                if (index <= this.InstructionPointer && this.InstructionPointer < index + item.Length)
                {
                    var color = Console.ForegroundColor;
                    Console.Write(item.Substring(0, this.InstructionPointer - index));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(item[this.InstructionPointer - index]);
                    Console.ForegroundColor = color;
                    Console.Write(item.Substring(this.InstructionPointer - index + 1));
                }
                else
                {
                    Console.Write(item);
                }

                index += item.Length;
            }

            InvertConsoleColor();
            Console.WriteLine();
        }

        private void PrintRow(int a, int b, int len, Func<byte, string> selector)
        {
            string empty = new string(' ', len);
            for (int i = a; i < b; i++)
            {
                if (i == this.CachePointer)
                {
                    InvertConsoleColor();
                }

                Console.Write(i < this.Cache.Length ? selector(this.Cache[i]).PadLeft(len) : empty);
                if (i == this.CachePointer)
                {
                    InvertConsoleColor();
                }
            }
        }

        private static void InvertConsoleColor()
        {
            var bg = Console.BackgroundColor;
            Console.BackgroundColor = Console.ForegroundColor;
            Console.ForegroundColor = bg;
        }
    }
}
