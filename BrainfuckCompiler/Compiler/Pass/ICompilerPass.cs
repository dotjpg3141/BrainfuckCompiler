using System.Collections.Generic;
using BrainfuckCompiler.Compiler.CodeGenerator;

namespace BrainfuckCompiler.Compiler.Pass
{
    public interface ICompilerPass
    {
        List<Instruction> Pass(List<Instruction> list);
    }
}
