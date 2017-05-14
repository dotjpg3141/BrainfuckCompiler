using System;
using System.Collections.Generic;
using System.Linq;

namespace BrainfuckCompiler.Compiler.Model
{
    public class Scope
    {
        public Scope(GlobalScope scope)
        {
            this.GlobalScope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public Scope(Scope scope)
        {
            this.ParentScope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.GlobalScope = scope.GlobalScope;
        }

        public GlobalScope GlobalScope { get; set; }

        public Scope ParentScope { get; set; }

        public List<Variable> Variables { get; } = new List<Variable>();

        public List<Function> Functions { get; } = new List<Function>();

        public Function AttachedFunction { get; set; }

        public bool NeedJumpInstruction { get; set; }

        public Variable FindLocalVariable(string name)
            => this.Variables.FirstOrDefault(v => v.Name == name);

        public Variable FindVariable(string name)
            => this.FindLocalVariable(name) ?? this.ParentScope?.FindVariable(name);

        public Function FindFunction(string name, DataType[] argumentTypes)
        {
            var localFunction = this.Functions.FirstOrDefault(
                f => f.Name == name && f.Parameters.Count == argumentTypes.Length);
            return localFunction ?? this.ParentScope?.FindFunction(name, argumentTypes);
        }

        public int HeapIndexOf(Variable variable)
            => this.GetHeapVariables().IndexOf(variable);

        public List<Variable> GetHeapVariables()
        {
            List<Variable> variables = new List<Variable>();
            foreach (var currentScope in this.GetScopeStack())
            {
                variables.AddRange(currentScope.Variables);
            }

            return variables;
        }

        public void DeclareVariable(Variable variable)
        {
            if (this.FindVariable(variable.Name) != null)
            {
                throw new CompilerException(variable.NameToken, $"variable {variable.Name} already declared");
            }

            this.Variables.Add(variable);
        }

        public void DeclareFunction(Function function)
        {
            var parameterTypes = function.Parameters.Select(param => param.Type).ToArray();
            if (this.FindFunction(function.Name, parameterTypes) != null)
            {
                throw new CompilerException(null, $"function '{function.Name}' already declared.");
            }

            function.Id = this.GlobalScope.GetNextLabelId();
            this.GlobalScope.FunctionById[function.Id] = function;
            this.Functions.Add(function);
        }

        private Stack<Scope> GetScopeStack()
        {
            Scope current = this;
            Stack<Scope> scopeStack = new Stack<Scope>();
            while (current != null)
            {
                scopeStack.Push(current);
                current = current.ParentScope;
            }

            return scopeStack;
        }
    }
}
