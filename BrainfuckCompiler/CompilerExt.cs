using System.Collections.Generic;
using BrainfuckCompiler.Compiler.CodeGenerator;

namespace BrainfuckCompiler
{
    public static class CompilerExt
    {
        public static void Add(this IList<Instruction> source, InstructionPrototype prototype, int[] args = null)
        {
            source.Add(new Instruction(prototype, args));
        }
    }
}
