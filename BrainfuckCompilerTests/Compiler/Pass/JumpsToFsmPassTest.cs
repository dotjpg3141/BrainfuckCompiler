using BrainfuckCompiler.Compiler.CodeGenerator;
using BrainfuckCompiler.Compiler.Pass;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BrainfuckCompiler.Compiler.CodeGenerator.InstructionPrototypes;

namespace BrainfuckCompilerTests.Compiler.Pass
{
    [TestClass]
    public class JumpsToFsmPassTest : PassTest
    {
        [TestMethod]
        public void PassJ2F_Empty()
        {
            this.AssertAreEqual(
                new JumpsToFsmPass(),
                new Instruction[0],
                new Instruction[0]);
        }

        [TestMethod]
        public void PassJ2F_NoJumpNoLabel()
        {
            this.AssertAreEqual(
                new JumpsToFsmPass(),
                new[]
                {
                    this.Insn(Nop, 0)
                },
                new[]
                {
                    this.Insn(Nop, 0)
                });
        }

        [TestMethod]
        public void PassJ2F_EndlesLoop()
        {
            this.AssertAreEqual(
                new JumpsToFsmPass(),
                new[]
                {
                    this.Insn(FsmBegin, 1),
                    this.Insn(FsmCase),
                    this.Insn(Nop, 0),
                    this.Insn(FsmJmp, 1),
                    this.Insn(FsmEnd)
                },
                new[]
                {
                   this.Insn(Label, 0),
                   this.Insn(Nop, 0),
                   this.Insn(Jump, 0)
                });
        }

        [TestMethod]
        public void PassJ2F_ConditionalJump()
        {
            this.AssertAreEqual(
                new JumpsToFsmPass(),
                new[]
                {
                    this.Insn(FsmBegin, 3),

                    this.Insn(FsmCase), // 1
                    this.Insn(ReadChar),
                    this.Insn(FsmIf, 3, 2),

                    this.Insn(FsmCase), // 2
                    this.Insn(Nop, 0),
                    this.Insn(FsmJmp, 3),

                    this.Insn(FsmCase),  // 3
                    this.Insn(Nop, 1),
                    this.Insn(FsmJmp, 4),

                    this.Insn(FsmEnd),
                },
                new[]
                {
                    this.Insn(ReadChar),
                    this.Insn(JumpIf, 0),
                    this.Insn(Nop, 0),
                    this.Insn(Label, 0),
                    this.Insn(Nop, 1)
                });
        }

        [TestMethod]
        public void PassJ2F_DoWhileLoop()
        {
            this.AssertAreEqual(
                new JumpsToFsmPass(),
                new[]
                {
                    this.Insn(FsmBegin, 1),
                    this.Insn(FsmCase),
                    this.Insn(Nop, 0),
                    this.Insn(PushInt, 7),
                    this.Insn(FsmIf, 1, 2),
                    this.Insn(FsmEnd),
                },
                new[]
                {
                    this.Insn(Label, 0),
                    this.Insn(Nop, 0),
                    this.Insn(PushInt, 7),
                    this.Insn(JumpIf, 0),
                });
        }

        [TestMethod]
        public void PassJ2F_Call()
        {
            this.AssertAreEqual(
                new JumpsToFsmPass(),
                new[]
                {
                    this.Insn(FsmBegin, 4),

                    // main
                    this.Insn(FsmCase), // 1
                    this.Insn(Nop, 1),
                    this.Insn(FsmCall, 3),

                    this.Insn(FsmCase), // 2
                    this.Insn(Nop, 2),
                    this.Insn(FsmJmp, 4),

                    // function
                    this.Insn(FsmCase), // 3
                    this.Insn(Nop, 11),
                    this.Insn(FsmReturn),

                    this.Insn(FsmCase), // 4
                    this.Insn(FsmJmp, 5),

                    this.Insn(FsmEnd),
                },
                new[]
                {
                    // main
                    this.Insn(Nop, 1),
                    this.Insn(Invoke, 0),
                    this.Insn(Nop, 2),
                    this.Insn(Jump, 1),

                    // function
                    this.Insn(Label, 0),
                    this.Insn(Nop, 11),
                    this.Insn(Return),

                    this.Insn(Label, 1)
                });
        }

        [TestMethod]
        public void PassJ2F_CallRecursive()
        {
            this.AssertAreEqual(
                new JumpsToFsmPass(),
                new[]
                {
                    this.Insn(FsmBegin, 5),

                    // main
                    this.Insn(FsmCase), // 1
                    this.Insn(Nop, 111),
                    this.Insn(FsmCall, 3),

                    this.Insn(FsmCase), // 2
                    this.Insn(Nop, 222),
                    this.Insn(FsmJmp, 5),

                    // function
                    this.Insn(FsmCase), // 3
                    this.Insn(Nop, 333),
                    this.Insn(FsmCall, 3),

                    this.Insn(FsmCase), // 4
                    this.Insn(Nop, 444),
                    this.Insn(FsmReturn),

                    this.Insn(FsmCase), // 5
                    this.Insn(FsmJmp, 6),

                    this.Insn(FsmEnd),
                },
                new[]
                {
                    // main
                    this.Insn(Nop, 111),
                    this.Insn(Invoke, 0),
                    this.Insn(Nop, 222),
                    this.Insn(Jump, 1),

                    // function
                    this.Insn(Label, 0),
                    this.Insn(Nop, 333),
                    this.Insn(Invoke, 0),
                    this.Insn(Nop, 444),
                    this.Insn(Return),

                    this.Insn(Label, 1),
                });
        }
    }
}
