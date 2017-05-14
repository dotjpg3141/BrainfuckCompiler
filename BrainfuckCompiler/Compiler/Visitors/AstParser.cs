using System;
using System.Linq;
using BrainfuckCompiler.Compiler.Model;

namespace BrainfuckCompiler.Compiler.Visitors
{
    public class AstParser
        : AstStatement.IStmtVisitor<Scope, AstStatement>,
        AstExpression.IExprVisitor<Scope, AstExpression>
    {
        #region Statements

        public void BeforeVisit(Scope context, AstStatement statement)
        {
            statement.Scope = context;
        }

        public AstStatement VisitBlock(Scope context, AstStatement.BlockStmt blockStmt)
        {
            var scope = new Scope(context);
            blockStmt.Scope = scope;
            for (int i = 0; i < blockStmt.Statements.Count; i++)
            {
                blockStmt.Statements[i] = blockStmt.Statements[i].Accept(scope, this);
            }

            return blockStmt;
        }

        public AstStatement VisitDoWhile(Scope context, AstStatement.DoWhileStmt doWhileStmt)
        {
            doWhileStmt.Body = doWhileStmt.Body.Accept(context, this);
            doWhileStmt.Condition = doWhileStmt.Condition.Accept(context, this);
            return doWhileStmt;
        }

        public AstStatement VisitExpression(Scope context, AstStatement.ExpressionStmt expressionStmt)
        {
            expressionStmt.Expression = expressionStmt.Expression.Accept(context, this);
            expressionStmt.Scope = context;
            return expressionStmt;
        }

        public AstStatement VisitFunction(Scope context, AstStatement.FunctionStmt functionStmt)
        {
            var functionScope = new Scope(context);

            foreach (var parameter in functionStmt.Parameter)
            {
                parameter.Type = DataTypes.GetFromName(parameter.TypeToken.Value);
                functionScope.DeclareVariable(parameter);
            }

            functionStmt.Function = new Function()
            {
                Name = functionStmt.Name.Value,
                Parameters = functionStmt.Parameter,
                ReturnType = DataTypes.GetFromName(functionStmt.Type.Value)
            };

            functionScope.AttachedFunction = functionStmt.Function;
            context.DeclareFunction(functionStmt.Function);

            functionStmt.Body = functionStmt.Body.Accept(functionScope, this);
            return functionStmt;
        }

        public AstStatement VisitIf(Scope context, AstStatement.IfStmt ifStmt)
        {
            ifStmt.Condition = ifStmt.Condition.Accept(context, this);
            ifStmt.TrueBranch = ifStmt.TrueBranch.Accept(context, this);
            ifStmt.FalseBranch = ifStmt.FalseBranch?.Accept(context, this);
            return ifStmt;
        }

        public AstStatement VisitReturn(Scope context, AstStatement.ReturnStmt returnStmt)
        {
            returnStmt.Expression = returnStmt.Expression?.Accept(context, this);
            return returnStmt;
        }

        public AstStatement VisitVariable(Scope context, AstStatement.VariableStmt variableStmt)
        {
            var expr = variableStmt.Expression.Accept(context, this);
            variableStmt.Expression = expr;
            variableStmt.Variable = new Variable()
            {
                NameToken = variableStmt.VariableName,
                Type = expr.Type,
            };
            variableStmt.Scope.DeclareVariable(variableStmt.Variable);
            return variableStmt;
        }

        public AstStatement VisitWhile(Scope context, AstStatement.WhileStmt whileStmt)
        {
            whileStmt.Condition = whileStmt.Condition.Accept(context, this);
            whileStmt.Body = whileStmt.Body.Accept(context, this);
            return whileStmt;
        }
        #endregion

        #region Expressions

        public void BeforeVisit(Scope context, AstExpression expression)
        {
        }

        public AstExpression VisitFunction(Scope context, AstExpression.Func func)
        {
            for (int i = 0; i < func.Arguments.Length; i++)
            {
                func.Arguments[i] = func.Arguments[i].Accept(context, this);
            }

            var argTypes = func.Arguments.Select(arg => arg.Type).ToArray();
            func.Function = context.FindFunction(func.TokenName.Value, argTypes) ?? throw new CompilerException(func.TokenName, $"cannot find function '{func.TokenName.Value}' with specified signature.");
            return func;
        }

        public AstExpression VisitLiteral(Scope context, AstExpression.Literal literal)
        {
            switch (literal.TokenName.Type)
            {
                case TokenType.Number:
                    literal.SetType(DataTypes.Int);
                    literal.Value = int.Parse(literal.TokenName.Value);
                    break;
                case TokenType.Char:
                    literal.SetType(DataTypes.Int);
                    literal.Value = (int)literal.TokenName.Value[0];
                    break;
                default:
                    throw new NotImplementedException();
            }

            return literal;
        }

        public AstExpression VisitVariable(Scope context, AstExpression.Var var)
        {
            var.Variable = context.FindVariable(var.TokenName.Value);
            if (var.Variable == null)
            {
                throw new CompilerException(var.TokenName, $"variable {var.TokenName.Value} is not defined.");
            }

            return var;
        }

        #endregion
    }
}
