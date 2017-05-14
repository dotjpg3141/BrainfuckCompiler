using System;

namespace BrainfuckCompiler.Brainfuck
{
    internal class DebugAction
    {
        public string Name { get; set; }

        public Action<BrainfuckInterpreter> Action { get; set; }

        public ConsoleKey Key { get; set; }
    }
}
