using System;
using System.Collections.Generic;
using BrainfuckCompiler.Compiler.Model;

namespace BrainfuckCompiler.Compiler
{
    public class AstGenerator
    {
        private static Dictionary<TokenType, int> operators = new Dictionary<TokenType, int>()
        {
            [TokenType.Assign] = 1,
            [TokenType.Equal] = 3,
            [TokenType.Greater] = 4,
            [TokenType.Add] = 5,
            [TokenType.Sub] = 5,
            [TokenType.Mul] = 6,
            [TokenType.Div] = 6,
        };

        public AstGenerator(Tokenizer tokenizer)
        {
            this.Tokenizer = tokenizer;
        }

        public Tokenizer Tokenizer { get; private set; }

        public AstStatement ParseCompilationUnit()
        {
            var block = new AstStatement.BlockStmt();
            while (this.Tokenizer.CanPeek())
            {
                block.Statements.Add(this.ParseStatement());
            }

            return block;
        }

        public AstStatement ParseStatement()
        {
            if (this.Tokenizer.CanPeek())
            {
                switch (this.Tokenizer.Peek().Type)
                {
                    case TokenType.Func:
                        return this.ParseFunction();
                    case TokenType.Return:
                        return this.ParseReturn();
                    case TokenType.Var:
                        return this.ParseVariable();
                    case TokenType.BracketBegin:
                        return this.ParseBlock();
                    case TokenType.While:
                        return this.ParseWhile();
                    case TokenType.Do:
                        return this.ParseDoWhile();
                    case TokenType.If:
                        return this.ParseIf();
                    default:
                        var exp = this.ParseExpression();
                        this.Consume(TokenType.StatementSeperator);
                        return new AstStatement.ExpressionStmt() { Expression = exp };
                }
            }

            throw new NotImplementedException();
        }

        public AstExpression ParseExpression()
        {
            return this.ParseExpression(0);
        }

        private AstExpression ParseExpression(int priority)
        {
            this.NextToken();
            AstExpression left = this.ParseSimpleExpression();

            while (this.Tokenizer.CanPeek()
                && operators.ContainsKey(this.Tokenizer.Peek().Type)
                && operators[this.Tokenizer.Peek().Type] >= priority)
            {
                var op = this.NextToken();
                var right = this.ParseExpression(operators[op.Type] + 1);
                left = new AstExpression.Func(op, left, right);
            }

            return left;
        }

        private AstExpression ParseSimpleExpression()
        {
            var current = this.Tokenizer.Current;
            switch (current.Type)
            {
                case TokenType.Number:
                case TokenType.Char:
                    return new AstExpression.Literal() { TokenName = current };
                case TokenType.Identifier:
                    // function call
                    if (this.Matches(TokenType.ParenthesisBegin))
                    {
                        var args = new List<AstExpression>();
                        if (!this.Matches(TokenType.ParenthesisEnd))
                        {
                            do
                            {
                                args.Add(this.ParseExpression());
                            }
                            while (this.Matches(TokenType.ArgumentSeperator));
                            this.Consume(TokenType.ParenthesisEnd);
                        }

                        return new AstExpression.Func(current, args.ToArray());
                    }

                    return new AstExpression.Var() { TokenName = current };
                case TokenType.ParenthesisBegin:
                    return this.ParseParenthesis();
                default:
                    throw new InvalidOperationException(
                        $"invalid token {current.Value}, expected number, identifier or '('");
            }
        }

        private AstExpression ParseParenthesis()
        {
            this.Assert(TokenType.ParenthesisBegin); // '('
            var exp = this.ParseExpression();
            this.Consume(TokenType.ParenthesisEnd); // ')'
            return exp;
        }

        private AstStatement ParseReturn()
        {
            this.Consume(TokenType.Return);
            AstExpression expr = null;
            if (!this.Matches(TokenType.StatementSeperator))
            {
                expr = this.ParseExpression();
                this.Consume(TokenType.StatementSeperator);
            }

            return new AstStatement.ReturnStmt() { Expression = expr };
        }

        private AstStatement ParseFunction()
        {
            this.Consume(TokenType.Func);
            this.Consume(TokenType.Identifier);
            var name = this.Tokenizer.Current;
            this.Consume(TokenType.ParenthesisBegin);
            var args = new List<Variable>();
            if (!this.Matches(TokenType.ParenthesisEnd))
            {
                args.Add(this.ParseParameter());
                while (this.Matches(TokenType.ArgumentSeperator))
                {
                    args.Add(this.ParseParameter());
                }

                this.Consume(TokenType.ParenthesisEnd);
            }

            Token returnType = null;
            if (this.Matches(TokenType.TypeSeperator))
            {
                returnType = this.ParseType();
            }

            var block = this.ParseStatement();
            return new AstStatement.FunctionStmt()
            {
                Name = name,
                Type = returnType,
                Parameter = args,
                Body = block,
            };
        }

        private Variable ParseParameter()
        {
            this.Consume(TokenType.Identifier);
            var name = this.Tokenizer.Current;
            this.Consume(TokenType.TypeSeperator);
            var type = this.ParseType();
            return new Variable()
            {
                NameToken = name,
                TypeToken = type,
            };
        }

        private Token ParseType()
        {
            this.Consume(TokenType.Identifier);
            return this.Tokenizer.Current;
        }

        private AstStatement ParseVariable()
        {
            this.Consume(TokenType.Var);
            this.Consume(TokenType.Identifier);
            var name = this.Tokenizer.Current;
            this.Consume(TokenType.Assign);
            var exp = this.ParseExpression();
            this.Consume(TokenType.StatementSeperator);
            return new AstStatement.VariableStmt() { VariableName = name, Expression = exp };
        }

        private AstStatement.BlockStmt ParseBlock()
        {
            this.Consume(TokenType.BracketBegin);
            var block = new AstStatement.BlockStmt();
            while (!this.Matches(TokenType.BracketEnd))
            {
                block.Statements.Add(this.ParseStatement());
            }

            return block;
        }

        private AstStatement.WhileStmt ParseWhile()
        {
            this.Consume(TokenType.While);
            this.Consume(TokenType.ParenthesisBegin);
            var exp = this.ParseParenthesis();
            var block = this.ParseStatement();
            return new AstStatement.WhileStmt() { Condition = exp, Body = block };
        }

        private AstStatement.DoWhileStmt ParseDoWhile()
        {
            this.Consume(TokenType.Do);
            var block = this.ParseStatement();
            this.Consume(TokenType.While);
            this.Consume(TokenType.ParenthesisBegin);
            var exp = this.ParseParenthesis();
            this.Consume(TokenType.StatementSeperator);
            return new AstStatement.DoWhileStmt() { Condition = exp, Body = block };
        }

        private AstStatement.IfStmt ParseIf()
        {
            this.Consume(TokenType.If);
            this.Consume(TokenType.ParenthesisBegin);
            var exp = this.ParseParenthesis();
            var trueBranch = this.ParseStatement();
            var falseBranch = this.Matches(TokenType.Else) ? this.ParseStatement() : null;
            return new AstStatement.IfStmt()
            {
                Condition = exp,
                TrueBranch = trueBranch,
                FalseBranch = falseBranch
            };
        }

        private void Consume(TokenType type)
        {
            this.Tokenizer.MoveNext();
            this.Assert(type);
        }

        private void Assert(TokenType type)
        {
            if (this.Tokenizer.EndOfStream || this.Tokenizer.Current.Type != type)
            {
                var value = this.Tokenizer.EndOfStream ? "EOF" : $"'{this.Tokenizer.Current.Value}'";
                throw new InvalidOperationException(
                    $"unexpected token {value}, expected {type}");
            }
        }

        private Token NextToken()
        {
            if (!this.Tokenizer.MoveNext())
            {
                throw new InvalidOperationException("expected token not EOF");
            }

            return this.Tokenizer.Current;
        }

        private bool Matches(TokenType type)
        {
            if (this.Tokenizer.CanPeek() && this.Tokenizer.Peek().Type == type)
            {
                this.Tokenizer.MoveNext();
                return true;
            }

            return false;
        }
    }
}
