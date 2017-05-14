namespace BrainfuckCompiler.Brainfuck
{
    public struct IntepreterInstruction
    {
        public static readonly IntepreterInstruction Invalid = new IntepreterInstruction()
        {
            Type = InterpreterInstructionType.Invalid
        };

        public InterpreterInstructionType Type;
        public int Value;
        public int SourceIndex;
        public int SourceLength;

        public IntepreterInstruction(InterpreterInstructionType type, int value, int sourceIndex)
            : this()
        {
            this.Type = type;
            this.Value = value;
            this.SourceIndex = sourceIndex;
            this.SourceLength = 1;
        }
    }
}
