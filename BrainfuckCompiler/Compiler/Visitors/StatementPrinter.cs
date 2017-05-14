using System.Linq;
using BrainfuckCompiler.Compiler.Model;

namespace BrainfuckCompiler.Compiler.Visitors
{
    public class StatementPrinter : AstStatement.IStmtVisitor<IndentWriter>
    {
        private InstructionGenerator instructionGenerator;

        public StatementPrinter()
        {
            this.instructionGenerator = new Visitors.InstructionGenerator();
        }

        public void VisitBlock(IndentWriter context, AstStatement.BlockStmt blockStmt)
        {
            context.Indent();
            foreach (var item in blockStmt.Statements)
            {
                item.Accept(context, this);
            }

            context.Unindent();
        }

        public void VisitDoWhile(IndentWriter context, AstStatement.DoWhileStmt doWhileStmt)
        {
            context.WriteLine("do {");
            context.Indent();
            doWhileStmt.Body.Accept(context, this);
            context.Unindent();
            context.WriteLine("} while (");
            context.Indent();
            this.PrintExpression(context, doWhileStmt.Condition, doWhileStmt.Scope);
            context.Unindent();
            context.WriteLine(");");
        }

        public void VisitExpression(IndentWriter context, AstStatement.ExpressionStmt expressionStmt)
        {
            this.PrintExpression(context, expressionStmt.Expression, expressionStmt.Scope);
        }

        public void VisitFunction(IndentWriter context, AstStatement.FunctionStmt functionStmt)
        {
            var fn = functionStmt.Function;
            context.WriteLine($"fn {fn.Name}[{string.Join(",", fn.Parameters.Select(p => p.Type.Name))}] {{");
            context.Indent();
            functionStmt.Body.Accept(context, this);
            context.Unindent();
            context.WriteLine("}");
        }

        public void VisitIf(IndentWriter context, AstStatement.IfStmt ifStmt)
        {
            context.WriteLine("if(");
            context.Indent();
            this.PrintExpression(context, ifStmt.Condition, ifStmt.Scope);
            context.Unindent();
            context.WriteLine(") {");

            context.Indent();
            ifStmt.TrueBranch.Accept(context, this);
            context.Unindent();

            if (ifStmt.FalseBranch != null)
            {
                context.WriteLine("} else {");
                context.Indent();
                ifStmt.FalseBranch.Accept(context, this);
                context.Unindent();
            }

            context.WriteLine("}");
        }

        public void VisitReturn(IndentWriter context, AstStatement.ReturnStmt returnStmt)
        {
            context.WriteLine($"return {{");
            context.Indent();
            this.PrintExpression(context, returnStmt.Expression, returnStmt.Scope);
            context.Unindent();
            context.WriteLine("}");
        }

        public void VisitWhile(IndentWriter context, AstStatement.WhileStmt whileStmt)
        {
            context.WriteLine("while(");
            context.Indent();
            this.PrintExpression(context, whileStmt.Condition, whileStmt.Scope);
            context.Unindent();
            context.WriteLine(") {");
            context.Indent();
            whileStmt.Body.Accept(context, this);
            context.Unindent();
            context.WriteLine("}");
        }

        public void VisitVariable(IndentWriter context, AstStatement.VariableStmt variableStmt)
        {
            context.WriteLine($"[{variableStmt.Variable.Type.Name}] {variableStmt.Variable.Name} = {{");
            context.Indent();
            this.PrintExpression(context, variableStmt.Expression, variableStmt.Scope);
            context.Unindent();
            context.WriteLine("}");
        }

        public void BeforeVisit(IndentWriter context, AstStatement statement)
        {
        }

        private void PrintExpression(IndentWriter context, AstExpression expression, Scope scope)
        {
            foreach (var item in expression.Accept(scope, this.instructionGenerator))
            {
                context.WriteLine(item);
            }
        }
    }
}
