using System.Diagnostics;

namespace BrainfuckCompiler.Compiler.Model
{
    public abstract class AstExpression
    {
        public interface IExprVisitor<in C, out T>
        {
            void BeforeVisit(C context, AstExpression expression);

            T VisitLiteral(C context, Literal literal);

            T VisitVariable(C context, Var var);

            T VisitFunction(C context, Func func);
        }

        public interface IExprVisitor<in C>
        {
            void BeforeVisit(C context, AstExpression expression);

            void VisitLiteral(C context, Literal literal);

            void VisitVariable(C context, Var var);

            void VisitFunction(C context, Func func);
        }

        public class Literal : AstExpression
        {
            private DataType type;

            public override DataType Type => this.type;

            public object Value { get; set; }

            public void SetType(DataType type) => this.type = type;

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IExprVisitor<C, T> visitor)
                => visitor.VisitLiteral(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IExprVisitor<C> visitor)
                => visitor.VisitLiteral(context, this);
        }

        public class Var : AstExpression
        {
            public Variable Variable { get; set; }

            public override DataType Type => this.Variable.Type;

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IExprVisitor<C, T> visitor)
                => visitor.VisitVariable(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IExprVisitor<C> visitor)
                => visitor.VisitVariable(context, this);
        }

        public class Func : AstExpression
        {
            public Func(Token token, params AstExpression[] arguments)
            {
                this.TokenName = token;
                this.Arguments = arguments;
            }

            public AstExpression[] Arguments { get; }

            public Function Function { get; set; }

            public override DataType Type => this.Function.ReturnType;

            [DebuggerNonUserCode]
            protected override T OnAccept<C, T>(C context, IExprVisitor<C, T> visitor)
                => visitor.VisitFunction(context, this);

            [DebuggerNonUserCode]
            protected override void OnAccept<C>(C context, IExprVisitor<C> visitor)
                => visitor.VisitFunction(context, this);
        }

        public Token TokenName { get; set; }

        public abstract DataType Type { get; }

        [DebuggerNonUserCode]
        public T Accept<C, T>(C context, IExprVisitor<C, T> visitor)
        {
            visitor.BeforeVisit(context, this);
            return this.OnAccept(context, visitor);
        }

        [DebuggerNonUserCode]
        public void Accept<C>(C context, IExprVisitor<C> visitor)
        {
            visitor.BeforeVisit(context, this);
            this.OnAccept(context, visitor);
        }

        protected abstract T OnAccept<C, T>(C context, IExprVisitor<C, T> visitor);

        protected abstract void OnAccept<C>(C context, IExprVisitor<C> visitor);
    }
}
