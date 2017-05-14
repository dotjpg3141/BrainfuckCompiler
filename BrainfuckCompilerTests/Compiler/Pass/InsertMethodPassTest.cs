using System.Collections.Generic;
using BrainfuckCompiler.Compiler.CodeGenerator;
using BrainfuckCompiler.Compiler.Model;
using BrainfuckCompiler.Compiler.Pass;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BrainfuckCompiler.Compiler.CodeGenerator.InstructionPrototypes;

namespace BrainfuckCompilerTests.Compiler.Pass
{
    [TestClass]
    public class InsertMethodPassTest : PassTest
    {
        [TestMethod]
        public void PassInsertMethod_Empty()
        {
            this.AssertAreEqual(this.CreatePass(), new Instruction[0], new Instruction[0]);
        }

        [TestMethod]
        public void PassInsertMethod_NoCall()
        {
            this.AssertAreEqual(
                this.CreatePass(),
                new[]
                {
                    this.Insn(Nop, 777),
                },
                new[]
                {
                    this.Insn(Nop, 777),
                });
        }

        [TestMethod]
        public void PassInsertMethod_Call()
        {
            var fn = new Function()
            {
                Body = new List<Instruction>()
                {
                    this.Insn(Nop, 333),
                    this.Insn(Return),
                },
                Parameters = new List<Variable>(),
            };

            var endLabel = 1;
            this.AssertAreEqual(
                this.CreatePass(fn),
                new[]
                {
                    // main
                    this.Insn(Nop, 111),
                    this.Insn(Invoke, fn.Id),
                    this.Insn(Nop, 222),

                    this.Insn(Jump, endLabel),

                    // function
                    this.Insn(Label, fn.Id),
                    this.Insn(Nop, 333),
                    this.Insn(Return),

                    this.Insn(Label, endLabel),
                },
                new[]
                {
                    this.Insn(Nop, 111),
                    this.Insn(Invoke, fn.Id),
                    this.Insn(Nop, 222),
                });
        }

        [TestMethod]
        public void PassInsertMethod_CallRecursive()
        {
            var invoke = this.Insn(Invoke, 0);
            var fn = new Function()
            {
                Body = new List<Instruction>()
                {
                    this.Insn(Nop, 333),
                    invoke,
                    this.Insn(Nop, 444),
                    this.Insn(Return),
                },
                Parameters = new List<Variable>(),
            };

            var pass = this.CreatePass(fn);
            invoke.Args[0] = fn.Id;

            var endLabel = 1;
            this.AssertAreEqual(
                pass,
                new[]
                {
                    // main
                    this.Insn(Nop, 111),
                    this.Insn(Invoke, fn.Id),
                    this.Insn(Nop, 222),

                    this.Insn(Jump, endLabel),

                    // function
                    this.Insn(Label, fn.Id),
                    this.Insn(Nop, 333),
                    this.Insn(Invoke, fn.Id),
                    this.Insn(Nop, 444),
                    this.Insn(Return),

                    this.Insn(Label, endLabel),
                },
                new[]
                {
                    this.Insn(Nop, 111),
                    this.Insn(Invoke, fn.Id),
                    this.Insn(Nop, 222),
                });
        }

        [TestMethod]
        public void PassInsertMethod_IndirectCall()
        {
            var fn1 = new Function { Name = "fn1", Parameters = new List<Variable>() };
            var fn2 = new Function { Name = "fn2", Parameters = new List<Variable>() };
            var pass = this.CreatePass(fn1, fn2);

            fn1.Body = new List<Instruction>()
            {
                this.Insn(Nop, 111),
                this.Insn(Invoke, fn2.Id),
                this.Insn(Nop, 222),
                this.Insn(Return),
            };

            fn2.Body = new List<Instruction>()
            {
                this.Insn(Nop, 333),
                this.Insn(Return),
            };

            var endLabel = 2;

            this.AssertAreEqual(
                pass,
                new Instruction[]
                {
                    // main
                    this.Insn(Nop, 444),
                    this.Insn(Invoke, fn1.Id),
                    this.Insn(Nop, 555),
                    this.Insn(Jump, endLabel),

                    // fn1
                    this.Insn(Label, fn1.Id),
                    this.Insn(Nop, 111),
                    this.Insn(Invoke, fn2.Id),
                    this.Insn(Nop, 222),
                    this.Insn(Return),

                    // fn2
                    this.Insn(Label, fn2.Id),
                    this.Insn(Nop, 333),
                    this.Insn(Return),

                    this.Insn(Label, endLabel),
                },
                new Instruction[]
                {
                    this.Insn(Nop, 444),
                    this.Insn(Invoke, fn1.Id),
                    this.Insn(Nop, 555),
                });
        }

        [TestMethod]
        public void PassInsertMethod_IndirectRecursion()
        {
            var fn1 = new Function { Name = "fn1", Parameters = new List<Variable>() };
            var fn2 = new Function { Name = "fn2", Parameters = new List<Variable>() };
            var pass = this.CreatePass(fn1, fn2);

            fn1.Body = new List<Instruction>()
            {
                this.Insn(Nop, 111),
                this.Insn(Invoke, fn2.Id),
                this.Insn(Nop, 222),
                this.Insn(Return),
            };

            fn2.Body = new List<Instruction>()
            {
                this.Insn(Nop, 333),
                this.Insn(Invoke, fn1.Id),
                this.Insn(Nop, 444),
                this.Insn(Return),
            };

            var endLabel = 2;

            this.AssertAreEqual(
                pass,
                new Instruction[]
                {
                    // main
                    this.Insn(Nop, 555),
                    this.Insn(Invoke, fn1.Id),
                    this.Insn(Nop, 666),

                    this.Insn(Jump, endLabel),

                    // fn1
                    this.Insn(Label, fn1.Id),
                    this.Insn(Nop, 111),
                    this.Insn(Invoke, fn2.Id),
                    this.Insn(Nop, 222),
                    this.Insn(Return),

                    // fn2
                    this.Insn(Label, fn2.Id),
                    this.Insn(Nop, 333),
                    this.Insn(Invoke, fn1.Id),
                    this.Insn(Nop, 444),
                    this.Insn(Return),

                    this.Insn(Label, endLabel),
                },
                new Instruction[]
                {
                    this.Insn(Nop, 555),
                    this.Insn(Invoke, fn1.Id),
                    this.Insn(Nop, 666),
                });
        }

        private InsertMethodsPass CreatePass(params Function[] functions)
        {
            var scope = GlobalScope.Generate();
            foreach (var fn in functions)
            {
                scope.DeclareFunction(fn);
            }

            return new InsertMethodsPass(scope.GlobalScope);
        }
    }
}
