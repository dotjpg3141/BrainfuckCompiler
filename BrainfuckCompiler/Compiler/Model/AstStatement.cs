using System.Collections.Generic;
using System.Diagnostics;

namespace BrainfuckCompiler.Compiler.Model
{
    public abstract class AstStatement
    {
        public interface IStmtVisitor<in C, out T>
        {
            void BeforeVisit(C context, AstStatement statement);

            T VisitExpression(C context, ExpressionStmt expressionStmt);

            T VisitBlock(C context, BlockStmt blockStmt);

            T VisitFunction(C context, FunctionStmt functionStmt);

            T VisitIf(C context, IfStmt ifStmt);

            T VisitWhile(C context, WhileStmt whileStmt);

            T VisitDoWhile(C context, DoWhileStmt doWhileStmt);

            T VisitVariable(C context, VariableStmt variableStmt);

            T VisitReturn(C context, ReturnStmt returnStmt);
        }

        public interface IStmtVisitor<in C>
        {
            void BeforeVisit(C context, AstStatement statement);

            void VisitExpression(C context, ExpressionStmt expressionStmt);

            void VisitBlock(C context, BlockStmt blockStmt);

            void VisitFunction(C context, FunctionStmt functionStmt);

            void VisitIf(C context, IfStmt ifStmt);

            void VisitWhile(C context, WhileStmt whileStmt);

            void VisitDoWhile(C context, DoWhileStmt doWhileStmt);

            void VisitVariable(C context, VariableStmt variableStmt);

            void VisitReturn(C context, ReturnStmt returnStmt);
        }

        public class ExpressionStmt : AstStatement
        {
            public AstExpression Expression { get; set; }

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IStmtVisitor<C, T> visitor)
                => visitor.VisitExpression(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IStmtVisitor<C> visitor)
                => visitor.VisitExpression(context, this);
        }

        public class BlockStmt : AstStatement
        {
            public List<AstStatement> Statements { get; set; } = new List<AstStatement>();

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IStmtVisitor<C, T> visitor)
                => visitor.VisitBlock(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IStmtVisitor<C> visitor)
                => visitor.VisitBlock(context, this);
        }

        public class FunctionStmt : AstStatement
        {
            public AstStatement Body { get; set; }

            public Token Name { get; set; }

            public Token Type { get; set; }

            public List<Variable> Parameter { get; set; }

            public Function Function { get; set; }

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IStmtVisitor<C, T> visitor)
                => visitor.VisitFunction(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IStmtVisitor<C> visitor)
                => visitor.VisitFunction(context, this);
        }

        public class IfStmt : AstStatement
        {
            public AstExpression Condition { get; set; }

            public AstStatement TrueBranch { get; set; }

            public AstStatement FalseBranch { get; set; }

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IStmtVisitor<C, T> visitor)
                => visitor.VisitIf(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IStmtVisitor<C> visitor)
                => visitor.VisitIf(context, this);
        }

        public class WhileStmt : AstStatement
        {
            public AstExpression Condition { get; set; }

            public AstStatement Body { get; set; }

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IStmtVisitor<C, T> visitor)
                => visitor.VisitWhile(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IStmtVisitor<C> visitor)
                => visitor.VisitWhile(context, this);
        }

        public class DoWhileStmt : AstStatement
        {
            public AstExpression Condition { get; set; }

            public AstStatement Body { get; set; }

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IStmtVisitor<C, T> visitor)
                => visitor.VisitDoWhile(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IStmtVisitor<C> visitor)
                => visitor.VisitDoWhile(context, this);
        }

        public class VariableStmt : AstStatement
        {
            public AstExpression Expression { get; set; }

            public Token VariableName { get; set; }

            public Variable Variable { get; set; }

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IStmtVisitor<C, T> visitor)
                => visitor.VisitVariable(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IStmtVisitor<C> visitor)
                => visitor.VisitVariable(context, this);
        }

        public class ReturnStmt : AstStatement
        {
            public AstExpression Expression { get; set; }

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IStmtVisitor<C, T> visitor)
                => visitor.VisitReturn(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IStmtVisitor<C> visitor)
                => visitor.VisitReturn(context, this);
        }

        public Scope Scope { get; set; }

        [DebuggerNonUserCode]
        public T Accept<C, T>(C context, IStmtVisitor<C, T> visitor)
        {
            visitor.BeforeVisit(context, this);
            return this.OnAccept(context, visitor);
        }

        [DebuggerNonUserCode]
        public void Accept<C>(C context, IStmtVisitor<C> visitor)
        {
            visitor.BeforeVisit(context, this);
            this.OnAccept(context, visitor);
        }

        protected abstract T OnAccept<C, T>(C context, IStmtVisitor<C, T> visitor);

        protected abstract void OnAccept<C>(C context, IStmtVisitor<C> visitor);
    }
}
