using System;
using System.Collections.Generic;
using System.IO;

namespace BrainfuckCompiler.Brainfuck
{
    public class OptimizedBrainfuckInterpreter
    {
        public TextReader Input { get; set; }

        public TextWriter Output { get; set; }

        public int CachePointer { get; set; }

        public int InstructionPointer { get; private set; }

        public char BreakpointChar { get; set; } = '#';

        private IntepreterInstruction[] CompileToInstructions(string source)
        {
            var instructions = new List<IntepreterInstruction>();
            var beginLoopIndeces = new Stack<int>();

            var pointerPosition = 0;
            var knownPosition = true;

            for (int i = 0; i < source.Length; i++)
            {
                var c = source[i];
                var lastType = instructions.Count == 0 ? InterpreterInstructionType.Invalid : instructions.Peek().Type;
                var nextInsn = IntepreterInstruction.Invalid;

                switch (c)
                {
                    case '+':
                    case '-':
                    case '<':
                    case '>':
                        HandlePointerAndValues(instructions, ref pointerPosition, knownPosition, i, c, lastType, ref nextInsn);
                        break;
                    case '[':
                        HandleBeginLoop(source, ref knownPosition, ref i, lastType, ref nextInsn);
                        break;
                    case ']':
                        nextInsn = HandleEndLoop(instructions, i);
                        break;
                    case '.':
                        nextInsn = new IntepreterInstruction(InterpreterInstructionType.Print, 0, i);
                        break;
                    case ',':
                        nextInsn = new IntepreterInstruction(InterpreterInstructionType.Read, 0, i);
                        break;
                    default:
                        if (c == this.BreakpointChar)
                        {
                            nextInsn = new IntepreterInstruction(InterpreterInstructionType.Breakpoint, 0, i);
                        }

                        break;
                }

                if (nextInsn.Type != InterpreterInstructionType.Invalid)
                {
                    instructions.Add(nextInsn);
                }
            }

            return instructions.ToArray();
        }

        private static IntepreterInstruction HandleEndLoop(List<IntepreterInstruction> instructions, int i)
        {
            IntepreterInstruction nextInsn;
            if (instructions.Count >= 2 &&
                  instructions[instructions.Count - 1].Type == InterpreterInstructionType.AddValue &&
                  (instructions[instructions.Count - 1].Value & 1) == 0 &&
                  instructions[instructions.Count - 2].Type == InterpreterInstructionType.BeginLoop)
            {
                var len = 1 + instructions.Pop().SourceLength + instructions.Pop().SourceLength;
                nextInsn = new IntepreterInstruction(InterpreterInstructionType.SetValue, 0, i + 1 - len);
                nextInsn.SourceLength = len;
            }
            else
            {
                nextInsn = new IntepreterInstruction(InterpreterInstructionType.EndLoop, 0, i);
            }

            return nextInsn;
        }

        private static void HandleBeginLoop(string source, ref bool knownPosition, ref int i, InterpreterInstructionType lastType, ref IntepreterInstruction nextInsn)
        {
            // skip loop, after loop
            if (lastType == InterpreterInstructionType.EndLoop)
            {
                i = ScannLoop(source, i, true);
            }
            else
            {
                // check if pointer movements in loops are balanced, so we can know the absolute position of the pointer
                if (knownPosition && !BalancedPointerInLoop(source, i))
                {
                    knownPosition = false;
                }

                nextInsn = new IntepreterInstruction(InterpreterInstructionType.BeginLoop, 0, i);
            }
        }

        private static void HandlePointerAndValues(List<IntepreterInstruction> instructions, ref int pointerPosition, bool knownPosition, int i, char c, InterpreterInstructionType lastType, ref IntepreterInstruction nextInsn)
        {
            var movePointer = c == '<' || c == '>';
            var changeValue = c == '+' || c == '-';
            if (movePointer || changeValue)
            {
                var step = c == '-' || c == '<' ? -1 : +1;
                if (movePointer)
                {
                    pointerPosition += step;
                }

                if ((movePointer && (lastType == InterpreterInstructionType.SetPointer || lastType == InterpreterInstructionType.MovePointer))
                 || (changeValue && (lastType == InterpreterInstructionType.SetValue || lastType == InterpreterInstructionType.AddValue)))
                {
                    nextInsn = instructions.Pop();
                    nextInsn.Value += step;
                    nextInsn.SourceLength++;
                }
                else if (movePointer)
                {
                    if (knownPosition)
                    {
                        nextInsn = new IntepreterInstruction(InterpreterInstructionType.SetPointer, pointerPosition, i);
                    }
                    else
                    {
                        nextInsn = new IntepreterInstruction(InterpreterInstructionType.MovePointer, pointerPosition, i);
                    }
                }
                else
                {
                    // if (changeValue)
                    nextInsn = new IntepreterInstruction(InterpreterInstructionType.AddValue, step, i);
                }
            }
        }

        private static bool BalancedPointerInLoop(string source, int startIndex)
        {
            var loops = new Stack<int>();
            var isBalanced = true;
            ScannLoop(source, startIndex, true, (index, chr, depth) =>
            {
                if (depth < loops.Count)
                {
                    loops.Push(0);
                }
                if (depth > loops.Count && loops.Pop() != 0)
                {
                    isBalanced = false;
                }

                if (chr == '<')
                {
                    loops.Push(loops.Pop() - 1);
                }
                else if (chr == '>')
                {
                    loops.Push(loops.Pop() + 1);
                }
            });
            return isBalanced;
        }

        private static int ScannLoop(string source, int startIndex, bool forward)
        {
            return ScannLoop(source, startIndex, forward, (index, chr, depth) => { });
        }

        private static int ScannLoop(string source, int startIndex, bool forward, Action<int, char, int> onInstruction)
        {
            int index = startIndex;
            int depth = 0;
            while (index >= 0 && index < source.Length)
            {
                if (source[index] == '[')
                {
                    depth++;
                }

                if (source[index] == ']')
                {
                    depth--;
                }

                onInstruction(index, source[index], Math.Abs(depth));

                if (depth == 0)
                {
                    break;
                }

                index += forward ? +1 : -1;
            }

            return index;
        }
    }
}
