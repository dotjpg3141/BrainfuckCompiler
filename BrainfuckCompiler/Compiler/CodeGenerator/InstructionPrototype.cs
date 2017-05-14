using BrainfuckCompiler.Compiler.Model;

namespace BrainfuckCompiler.Compiler.CodeGenerator
{
    public class InstructionPrototype
    {
        public InstructionPrototype(string name, int argCount, DataType[] input, DataType[] output)
        {
            this.Name = name;
            this.ArgumentCount = argCount;
            this.InputTypes = input;
            this.OutputTypes = output;
        }

        public string Name { get; }

        public int ArgumentCount { get; }

        internal DataType[] InputTypes { get; }

        internal DataType[] OutputTypes { get;  }
    }
}
