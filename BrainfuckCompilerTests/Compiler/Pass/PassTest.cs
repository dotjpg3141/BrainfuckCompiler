using System.Collections.Generic;
using System.Linq;
using BrainfuckCompiler.Compiler.CodeGenerator;
using BrainfuckCompiler.Compiler.Pass;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrainfuckCompilerTests.Compiler.Pass
{
    public abstract class PassTest
    {
        public void AssertAreEqual(ICompilerPass pass, Instruction[] expected, Instruction[] passInput)
        {
            var actual = pass.Pass(new List<Instruction>(passInput));
            var msg = $@"
input    = {GetInstructionString(passInput)}
expected = {GetInstructionString(expected)}
actual   = {GetInstructionString(actual)}
";
            Assert.IsTrue(expected.SequenceEqual(actual), msg);
        }

        protected static string GetInstructionString(IEnumerable<Instruction> instructions)
            => "[" + string.Join(", ", instructions) + "]";

        protected Instruction Insn(InstructionPrototype proto, params int[] args)
            => new Instruction(proto, args);
    }
}
