using System.IO;
using System.Linq;
using BrainfuckCompiler.Compiler;
using BrainfuckCompiler.Compiler.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrainfuckCompilerTests.Compiler
{
    [TestClass]
    public class AstGeneratorExpressionTest
    {
        [TestMethod]
        public void AstExp_Number()
        {
            var expr = "123";
            AssertExprEqual(expr, "123");
        }

        [TestMethod]
        public void AstExp_Char()
        {
            var expr = "a";
            AssertExprEqual(expr, "'a'");
        }

        [TestMethod]
        public void AstExp_Parenthesis()
        {
            var expr = "123";
            AssertExprEqual(expr, "(123)");
        }

        [TestMethod]
        public void AstExp_Add()
        {
            var expr = new Tree("f:+", "1", "2");
            AssertExprEqual(expr, "1+2");
        }

        [TestMethod]
        public void AstExp_Sub()
        {
            var expr = new Tree("f:-", "1", "2");
            AssertExprEqual(expr, "1-2");
        }

        [TestMethod]
        public void AstExp_LeftAssociative()
        {
            var expr = new Tree("f:+", new Tree("f:+", "1", "2"), "3");
            AssertExprEqual(expr, "1+2+3");
        }

        [TestMethod]
        public void AstExp_Precedence()
        {
            var expr1 = new Tree("f:+", "1", new Tree("f:*", "2", "3"));
            AssertExprEqual(expr1, "1+2*3");
            AssertExprEqual(expr1, "1+(2*3)");

            var expr2 = new Tree("f:+", new Tree("f:*", "1", "2"), "3");
            AssertExprEqual(expr2, "1*2+3");
            AssertExprEqual(expr2, "(1*2)+3");

            var expr3 = new Tree("f:*", "1", new Tree("f:+", "2", "3"));
            AssertExprEqual(expr3, "1*(2+3)");

            var expr4 = new Tree("f:*", new Tree("f:+", "1", "2"), "3");
            AssertExprEqual(expr4, "(1+2)*3");
        }

        [TestMethod]
        public void AstExp_Variable()
        {
            var expr = "v:pi";
            AssertExprEqual(expr, "pi");
        }

        [TestMethod]
        public void AstExp_MethodWithNoArgs()
        {
            var expr = "f:rnd";
            AssertExprEqual(expr, "rnd()");
        }

        [TestMethod]
        public void AstExp_MethodWithOneArg()
        {
            var expr = new Tree("f:abs", "7");
            AssertExprEqual(expr, "abs(7)");
        }

        [TestMethod]
        public void AstExp_MethodWithTwoArgs()
        {
            var expr = new Tree("f:max", "1", "3");
            AssertExprEqual(expr, "max(1,3)");
        }

        private static AstGenerator NewAstGenerator(string source)
        {
            var reader = new CharReader(new StringReader(source));
            var astgen = new AstGenerator(new Tokenizer(reader));
            return astgen;
        }

        private static void AssertExprEqual(Tree expected, string source)
        {
            var astgen = NewAstGenerator(source);
            var actualExpr = astgen.ParseExpression();
            var actual = actualExpr.Accept(null, new ExpressionTreeVisitor());

            var msg = $"actual = {actual.ToString()}\nexpected = {expected.ToString()}";
            Tree.AssertEqual(expected, actual, (lhs, rhs) => lhs == rhs, msg);

            Assert.IsFalse(astgen.Tokenizer.CanPeek(), "end of stream");
        }

        private class ExpressionTreeVisitor : AstExpression.IExprVisitor<object, Tree>
        {
            public Tree VisitFunction(object c, AstExpression.Func func)
                => new Tree("f:" + func.TokenName.Value, func.Arguments.Select(p => p.Accept(c, this)).ToArray());

            public Tree VisitLiteral(object c, AstExpression.Literal literal)
                => new Tree(string.Empty + literal.TokenName.Value);

            public Tree VisitVariable(object c, AstExpression.Var var)
                => new Tree("v:" + var.TokenName.Value);

            public void BeforeVisit(object context, AstExpression expression)
            {
            }
        }
    }
}
