using System;
using System.Collections.Generic;
using System.IO;
using BrainfuckCompiler;
using BrainfuckCompiler.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrainfuckCompilerTests
{
    [TestClass]
    public class BfcTest
    {
        [TestMethod]
        public void Bfc_Print()
        {
            var source = this.Compile("print(48);");
            Assert.AreEqual("0", this.Execute(source));

            source = this.Compile("print('0');");
            Assert.AreEqual("0", this.Execute(source));
        }

        [TestMethod]
        public void Bfc_Read()
        {
            var source = this.Compile("print(read());");
            Assert.AreEqual("0", this.Execute(source, "0"));
            Assert.AreEqual(".", this.Execute(source, "."));
        }

        [TestMethod]
        public void Bfc_PrintNumber()
        {
            var source = this.Compile("printNumber(1);");
            Assert.AreEqual("1", this.Execute(source));

            source = this.Compile("printNumber(23);");
            Assert.AreEqual("23", this.Execute(source));

            source = this.Compile("printNumber(255);");
            Assert.AreEqual("255", this.Execute(source));
        }

        [TestMethod]
        public void Bfc_ReadDigit()
        {
            var source = this.Compile("printNumber(readDigit());");
            Assert.AreEqual("0", this.Execute(source, "0"));
            Assert.AreEqual("3", this.Execute(source, "3"));
            Assert.AreEqual("9", this.Execute(source, "9"));
        }

        [TestMethod]
        public void Bfc_Operators()
        {
            var operators = new Dictionary<string, int>()
            {
                ["+"] = 9,
                ["-"] = 3,
                ["*"] = 18,
                ["/"] = 2,
            };

            foreach (var op in operators)
            {
                var source = this.Compile($"print(6{op.Key}3);");
                Assert.AreEqual(((char)op.Value).ToString(), this.Execute(source), "Operator: " + op.Key);
            }
        }

        [TestMethod]
        public void Bfc_If()
        {
            var source = this.Compile("if(read()) { print(48); }");
            Assert.AreEqual(string.Empty, this.Execute(source, "\0"));
            Assert.AreEqual("0", this.Execute(source, "A"));
        }

        [TestMethod]
        public void Bfc_IfElse()
        {
            var source = this.Compile("if(read()) { print(48); } else { print(49); }");
            Assert.AreEqual("0", this.Execute(source, "A"));
            Assert.AreEqual("1", this.Execute(source, "\0"));
        }

        [TestMethod]
        public void Bfc_While()
        {
            var source = this.Compile("while(read()==49) { print(48); }");
            Assert.AreEqual(string.Empty, this.Execute(source, "A"));
            Assert.AreEqual("0", this.Execute(source, "1A"));
            Assert.AreEqual("00", this.Execute(source, "11A"));
        }

        [TestMethod]
        public void Bfc_DoWhile()
        {
            var source = this.Compile("do { print(48); } while (read()==49);");
            Assert.AreEqual("0", this.Execute(source, "A"));
            Assert.AreEqual("00", this.Execute(source, "1A"));
            Assert.AreEqual("000", this.Execute(source, "11A"));
        }

        [TestMethod]
        public void Bfc_DeclareVariable()
        {
            var source = this.Compile("var a = 48; print(a);");
            Assert.AreEqual("0", this.Execute(source));

            var source2 = this.Compile("var a = 48; print(a); print(a);");
            Assert.AreEqual("00", this.Execute(source2));
        }

        [TestMethod]
        public void Bfc_AsignVariable()
        {
            var source = this.Compile("var a = 48; print(a); a = a + 1; print(a);");
            Assert.AreEqual("01", this.Execute(source));
        }

        [TestMethod]
        public void Bfc_Scope()
        {
            var source = this.Compile("var a = '0'; { print(a); }");
            Assert.AreEqual("0", this.Execute(source));

            source = this.Compile("{ var a = '0'; print(a); } { var a = '1'; print(a); }");
            Assert.AreEqual("01", this.Execute(source));

            AssertException<CompilerException>(() => this.Compile("var a = '0'; { var a = '1'; }"));
            AssertException<CompilerException>(() => this.Compile("{ var a = '0'; } print(a); "));
        }

        [TestMethod]
        public void Bfc_Function()
        {
            var source = this.Compile("func id(x:int):int { return x; } print(id('0'));");
            Assert.AreEqual("0", this.Execute(source));
        }

        [TestMethod]
        public void Bfc_Function_TwoCalls()
        {
            var source = this.Compile("func id(x:int):int { return x; } print(id('0')); print(id('1'));");
            Assert.AreEqual("01", this.Execute(source));
        }

        [TestMethod]
        public void Bfc_Function_Recursive_Sum()
        {
            var source = this.Compile(@"
func sum(n:int):int {
    if (n) {
        n = n + sum(n - 1);
    }
    return n;
}
printNumber(sum(readDigit()));
");
            Assert.AreEqual("0", this.Execute(source, "0"));
            Assert.AreEqual("1", this.Execute(source, "1"));
            Assert.AreEqual("6", this.Execute(source, "3"));
            Assert.AreEqual("15", this.Execute(source, "5"));
        }

        [TestMethod]
        public void Bfc_Greater()
        {
            var source = this.Compile("print(1 > 2);");
            Assert.AreEqual("\0", this.Execute(source));
        }

        public string Compile(string source)
        {
            var brainfuckCode = new StringWriter();
            Program.Compile(new StringReader(source), brainfuckCode, verbose: false);
            return brainfuckCode.ToString();
        }

        public string Execute(string bfScource, string input = "")
        {
            var interpreter = new BrainfuckCompiler.Brainfuck.BrainfuckInterpreter(bfScource, new byte[256])
            {
                Input = new StringReader(input),
                Output = new StringWriter(),
            };
            interpreter.Run();
            return interpreter.Output.ToString();
        }

        private static void AssertException<T>(Action action)
            where T : Exception
        {
            try
            {
                action();
                Assert.Fail($"expected exception of type {typeof(T).Name}.");
            }
            catch (T)
            {
                // nop
            }
        }
    }
}
