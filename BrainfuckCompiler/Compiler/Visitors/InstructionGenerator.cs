using System;
using System.Collections.Generic;
using System.Linq;
using BrainfuckCompiler.Compiler.CodeGenerator;
using BrainfuckCompiler.Compiler.Model;
using static BrainfuckCompiler.Compiler.CodeGenerator.InstructionPrototypes;

namespace BrainfuckCompiler.Compiler.Visitors
{
    public class InstructionGenerator
        : AstStatement.IStmtVisitor<object, List<Instruction>>,
        AstExpression.IExprVisitor<Scope, List<Instruction>>
    {
        #region Statements
        public void BeforeVisit(object context, AstStatement statement)
        {
        }

        public List<Instruction> VisitBlock(object context, AstStatement.BlockStmt blockStmt)
        {
            List<Instruction> list = new List<Instruction>();
            list.Add(UpdateScope, blockStmt.Scope.GetHeapVariables()
                .Select(var => var.Type.Id)
                .ToArray());
            list.AddRange(blockStmt.Statements
                .SelectMany(stmt => stmt.Accept(context, this))
                .ToList());
            return list;
        }

        public List<Instruction> VisitDoWhile(object context, AstStatement.DoWhileStmt doWhileStmt)
        {
            /*
             * do ( stmt() ) while (expr());
             *
             * #label
             * stmt();
             * expr();
             * jumpIf #label
             */

            var label = this.GetNextLabel(doWhileStmt);
            var list = new List<Instruction>();
            list.Add(Label, new[] { label });
            list.AddRange(doWhileStmt.Body.Accept(context, this));
            list.AddRange(doWhileStmt.Condition.Accept(doWhileStmt.Scope, this));
            list.Add(JumpIf, new[] { label });
            return list;
        }

        public List<Instruction> VisitExpression(object context, AstStatement.ExpressionStmt expressionStmt)
        {
            var instructions = expressionStmt.Expression.Accept(expressionStmt.Scope, this);

            // pop return value if type is not void
            if (expressionStmt.Expression.Type != DataTypes.Void)
            {
                if (expressionStmt.Expression.Type == DataTypes.Int)
                {
                    instructions.Add(PopInt);
                }
                else
                {
                    throw new NotImplementedException(
                        "invalid return type " + expressionStmt.Expression.Type);
                }
            }

            return instructions;
        }

        public List<Instruction> VisitFunction(object context, AstStatement.FunctionStmt functionStmt)
        {
            functionStmt.Function.Body = functionStmt.Body.Accept(context, this);
            return new List<Instruction>();
        }

        public List<Instruction> VisitIf(object context, AstStatement.IfStmt ifStmt)
        {
            // var list = new List<Instruction>();
            // list.AddRange(ifStmt.Condition.Accept(this));
            // list.Add(IfElseBegin));
            // list.AddRange(ifStmt.TrueBranch.Accept(this));
            // list.Add(IfElseElse));
            // if (ifStmt.FalseBranch != null)
            // {
            //    list.AddRange(ifStmt.FalseBranch.Accept(this));
            // }
            // list.Add(IfElseEnd));
            // return list;

            /*
                 * if (condition()) { statement(); }
                 * 1)   condition();
                 *      jmpif 2 3
                 * 2)   statement();
                 *      jmp 3
                 * 3)   ...
                 */
            /*
             * if (condition()) { statement1(); } else { statement2(); }
             * 1)   condition();
             *      jmpif 2 3
             * 2)   statement1();
             *      jmp 4
             * 3)   statement2();
             *      jmp 4
             * 4)   ...

             */

            var scope = ifStmt.Scope;
            if (ifStmt.FalseBranch == null)
            {
                /*
                 * if (condition()) { statement(); }
                 *
                 * condition();
                 * not[]
                 * jumpif #label
                 * statement();
                 * #label
                 */
                var label = this.GetNextLabel(ifStmt);
                var list = new List<Instruction>();
                list.AddRange(ifStmt.Condition.Accept(scope, this));
                list.Add(Not);
                list.Add(JumpIf, new int[] { label });
                list.AddRange(ifStmt.TrueBranch.Accept(context, this));
                list.Add(Label, new int[] { label });
                return list;
            }
            else
            {
                /*
                 * if (condition()) { statement1(); } else { statement2(); }
                 *
                 * condition();
                 * jumpif #labelTrue
                 * statement2();
                 * jump #labelEnd
                 * #labelTrue
                 * statement1();
                 * #labelEnd
                 */
                var labelTrue = this.GetNextLabel(ifStmt);
                var labelEnd = this.GetNextLabel(ifStmt);
                var list = new List<Instruction>();
                list.AddRange(ifStmt.Condition.Accept(scope, this));
                list.Add(JumpIf, new int[] { labelTrue });
                list.AddRange(ifStmt.FalseBranch.Accept(context, this));
                list.Add(Jump, new int[] { labelEnd });
                list.Add(Label, new int[] { labelTrue });
                list.AddRange(ifStmt.TrueBranch.Accept(context, this));
                list.Add(Label, new int[] { labelEnd });
                return list;
            }
        }

        public List<Instruction> VisitReturn(object context, AstStatement.ReturnStmt returnStmt)
        {
            var scope = returnStmt.Scope;
            var returnAddress = scope.HeapIndexOf(scope.GlobalScope.ReturnAddress);

            var list = new List<Instruction>();
            list.Add(WriteLocal, new int[] { returnAddress });
            list.AddRange(returnStmt.Expression.Accept(scope, this));
            list.Add(ReadLocal, new int[] { returnAddress });
            return list;
        }

        public List<Instruction> VisitVariable(object context, AstStatement.VariableStmt variableStmt)
        {
            var scope = variableStmt.Scope;
            var list = variableStmt.Expression.Accept(scope, this);
            if (variableStmt.Variable.Type != DataTypes.Int)
            {
                throw new InvalidOperationException("only integer type expected as variable for now");
            }

            int id = scope.HeapIndexOf(variableStmt.Variable);
            list.Add(WriteLocal, new int[] { id });

            return list;
        }

        public List<Instruction> VisitWhile(object context, AstStatement.WhileStmt whileStmt)
        {
            /*
             * while ( condition() ) { statement(); }
             *
             * #labelStart
             * condition()
             * not[]
             * jumpIf #labelEnd
             * statement()
             * jump #labelStart
             * #labelEnd
             */
            var labelStart = this.GetNextLabel(whileStmt);
            var labelEnd = this.GetNextLabel(whileStmt);

            var list = new List<Instruction>();
            list.Add(Label, new int[] { labelStart });
            list.AddRange(whileStmt.Condition.Accept(whileStmt.Scope, this));
            list.Add(Not);
            list.Add(JumpIf, new int[] { labelEnd });
            list.AddRange(whileStmt.Body.Accept(context, this));
            list.Add(Jump, new int[] { labelStart });
            list.Add(Label, new int[] { labelEnd });
            return list;
        }
        #endregion

        #region Expressions
        public void BeforeVisit(Scope context, AstExpression expression)
        {
        }

        public List<Instruction> VisitFunction(Scope context, AstExpression.Func func)
        {
            if (func.TokenName.Value == "="
                && func.Arguments.Length == 2
                && func.Arguments[0].Type == func.Arguments[1].Type)
            {
                if (func.Arguments[0].Type != DataTypes.Int)
                {
                    throw new NotImplementedException();
                }

                var variableExpression = func.Arguments[0] as AstExpression.Var;
                if (variableExpression == null)
                {
                    throw new Exception("Left hand side of operator '=' must be a variable");
                }

                var list = func.Arguments[1].Accept(context, this);
                list.Add(DupInt);
                list.Add(WriteLocal, new int[] { context.HeapIndexOf(variableExpression.Variable) });
                return list;
            }
            else if (func.Function.IsBuildIn)
            {
                var list = func.Arguments.SelectMany(arg => arg.Accept(context, this)).ToList();
                list.AddRange(func.Function.Body);
                return list;
            }
            else
            {
                var list = func.Arguments.SelectMany(arg => arg.Accept(context, this)).ToList();
                list.Add(Invoke, new int[] { func.Function.Id });
                return list;
            }
        }

        public List<Instruction> VisitLiteral(Scope context, AstExpression.Literal literal)
        {
            if (literal.Type == DataTypes.Int)
            {
                return new List<Instruction>()
                {
                    new Instruction(PushInt, new[] { (int)literal.Value })
                };
            }

            throw new NotImplementedException();
        }

        public List<Instruction> VisitVariable(Scope context, AstExpression.Var var)
        {
            if (var.Type == DataTypes.Int)
            {
                int heapIndex = context.HeapIndexOf(var.Variable);
                return new List<Instruction>()
                {
                    new Instruction(ReadLocal, new[] { heapIndex }),
                    new Instruction(DupInt),
                    new Instruction(WriteLocal, new[] { heapIndex }),
                };
            }

            throw new NotImplementedException();
        }
        #endregion

        private int GetNextLabel(AstStatement stmt) => stmt.Scope.GlobalScope.GetNextLabelId();
    }
}
