namespace BrainfuckCompiler.Brainfuck
{
    public enum InterpreterInstructionType
    {
        Invalid = -1,
        SetPointer,
        MovePointer,
        SetValue,
        AddValue,
        BeginLoop,
        EndLoop,
        Print,
        Read,
        Breakpoint
    }
}
