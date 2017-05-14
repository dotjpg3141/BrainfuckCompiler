using System.IO;
using System.Linq;
using BrainfuckCompiler.Compiler;
using BrainfuckCompiler.Compiler.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrainfuckCompilerTests.Compiler
{
    [TestClass]
    public class AstGeneratorStatementTest
    {
        private readonly Tree block = new Tree("block", "stmt");

        [TestMethod]
        public void AstStmt_Expr()
        {
            var stmt = "stmt";
            AssertStmtEqual(stmt, "test();");
        }

        [TestMethod]
        public void AstStmt_Block()
        {
            var stmt = new Tree("block", "stmt", "stmt");
            AssertStmtEqual(stmt, "{stmt1();stmt2();}");
        }

        [TestMethod]
        public void AstStmt_If()
        {
            var stmt = new Tree("if", "expr", this.block, null);
            AssertStmtEqual(stmt, "if(0){stmt();}");
        }

        [TestMethod]
        public void AstStmt_IfElse()
        {
            var stmt = new Tree("if", "expr", this.block, this.block);
            AssertStmtEqual(stmt, "if(0){stmt1();}else{stmt2();}");
        }

        [TestMethod]
        public void AstStmt_While()
        {
            var stmt = new Tree("while", "expr", this.block);
            AssertStmtEqual(stmt, "while(1){stmt();}");
        }

        [TestMethod]
        public void AstStmt_DoWhile()
        {
            var stmt = new Tree("dowhile", this.block, "expr");
            AssertStmtEqual(stmt, "do{stmt();}while(1);");
        }

        [TestMethod]
        public void AstStmt_ReturnValue()
        {
            var stmt = new Tree("return", "expr");
            AssertStmtEqual(stmt, "return 1;");
        }

        [TestMethod]
        public void AstStmt_ReturnVoid()
        {
            var stmt = new Tree("return", (Tree)null);
            AssertStmtEqual(stmt, "return;");
        }

        [TestMethod]
        public void AstStmt_Var()
        {
            var stmt = new Tree("var", "expr");
            AssertStmtEqual(stmt, "var foo = 3;");
        }

        [TestMethod]
        public void AstStmt_Function()
        {
            var stmt = new Tree("func", this.block);
            AssertStmtEqual(stmt, "func foo(a:int):int {stmt();}");
        }

        private static AstGenerator NewAstGenerator(string source)
        {
            var reader = new CharReader(new StringReader(source));
            var astgen = new AstGenerator(new Tokenizer(reader));
            return astgen;
        }

        private static void AssertStmtEqual(Tree expected, string source)
        {
            var astgen = NewAstGenerator(source);
            var actualStmt = astgen.ParseStatement();
            var actual = actualStmt.Accept(null, new TreeTestVisitor());

            var msg = $"actual = {actual.ToString()}\nexpected = {expected.ToString()}";
            Tree.AssertEqual(expected, actual, (lhs, rhs) => lhs == rhs, msg);

            Assert.IsFalse(astgen.Tokenizer.CanPeek(), "end of stream");
        }

        public class TreeTestVisitor : AstStatement.IStmtVisitor<object, Tree>
        {
            public Tree VisitBlock(object c, AstStatement.BlockStmt blockStmt)
                => new Tree("block", blockStmt.Statements.Select(s => s.Accept(c, this)).ToArray());

            public Tree VisitDoWhile(object c, AstStatement.DoWhileStmt doWhileStmt)
                => new Tree("dowhile", doWhileStmt.Body.Accept(c, this), Expr(doWhileStmt.Condition));

            public Tree VisitExpression(object c, AstStatement.ExpressionStmt expressionStmt)
                => new Tree("stmt");

            public Tree VisitFunction(object c, AstStatement.FunctionStmt functionStmt)
                => new Tree("func", functionStmt.Body.Accept(c, this));

            public Tree VisitIf(object c, AstStatement.IfStmt ifStmt)
                => new Tree("if", Expr(ifStmt.Condition), ifStmt.TrueBranch.Accept(c, this), ifStmt.FalseBranch?.Accept(c, this));

            public Tree VisitReturn(object c, AstStatement.ReturnStmt returnStmt)
                => new Tree("return", Expr(returnStmt.Expression));

            public Tree VisitVariable(object c, AstStatement.VariableStmt variableStmt)
                => new Tree("var", Expr(variableStmt.Expression));

            public Tree VisitWhile(object c, AstStatement.WhileStmt whileStmt)
                => new Tree("while", Expr(whileStmt.Condition), whileStmt.Body.Accept(c, this));

            public void BeforeVisit(object context, AstStatement statement)
            {
            }

            private static Tree Expr(AstExpression expr)
                => expr == null ? null : new Tree("expr");
        }
    }
}
