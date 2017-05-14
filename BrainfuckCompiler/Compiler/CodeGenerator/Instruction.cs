using System;
using System.Linq;
using BrainfuckCompiler.Compiler.Model;

namespace BrainfuckCompiler.Compiler.CodeGenerator
{
    public sealed class Instruction
    {
        public Instruction(InstructionPrototype prototype, int[] args = null)
        {
            this.Prototype = prototype;
            this.Args = args ?? new int[0];
            if (this.Prototype.ArgumentCount != -1 && this.Args.Length != this.Prototype.ArgumentCount)
            {
                throw new ArgumentException($@"illegal argument count for instruction {
                    prototype.Name}, got {this.Args.Length}, expected {this.Prototype.ArgumentCount}");
            }
        }

        public InstructionPrototype Prototype { get; }

        public int[] Args { get; }

        public DataType[] GetStackInputTypes()
        {
            if (this.Prototype == InstructionPrototypes.Invoke)
            {
                return this.GetAsFunction(0).Parameters.Select(var => var.Type).ToArray();
            }

            if (this.Prototype.InputTypes == null)
            {
                throw new NotImplementedException("cannot retrieve input types");
            }

            return this.Prototype.InputTypes;
        }

        public DataType[] GetStackOutputTypes()
        {
            if (this.Prototype == InstructionPrototypes.Invoke)
            {
                return new DataType[] { this.GetAsFunction(0).ReturnType };
            }

            if (this.Prototype.OutputTypes == null)
            {
                throw new NotImplementedException("cannot retrieve ouput types");
            }

            return this.Prototype.OutputTypes;
        }

        public int Get(int index)
        {
            return this.Args[index];
        }

        public Function GetAsFunction(int index)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.Prototype.Name + "[" + string.Join(",", this.Args) + "]";
        }

        public override bool Equals(object obj)
        {
            var other = obj as Instruction;
            return other != null && Equal.New
                .Compare(this.Prototype, other.Prototype)
                .CompareEnumerable(this.Args, other.Args);
        }

        public override int GetHashCode()
            => Hash.New.Add(this.Prototype).AddEnumerable(this.Args);
    }
}
