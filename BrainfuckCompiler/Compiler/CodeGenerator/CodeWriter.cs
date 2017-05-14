using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BrainfuckCompiler.Compiler.Model;
using InsnProtos = BrainfuckCompiler.Compiler.CodeGenerator.InstructionPrototypes;

namespace BrainfuckCompiler.Compiler.CodeGenerator
{
    // TODO replace string functions with void functions + Write
    public class CodeWriter
    {
        private const string Breakpoint = "#";
        private const string FsmEndCase = ">]<";

        private int depthLevel = 0;
        private Stack<int> fsmCaseNumber = new Stack<int>();
        private Stack<int> fsmCaseCount = new Stack<int>();
        private List<DataType> currentVariableTypes;

        public CodeWriter(TextWriter writer)
        {
            this.Writer = writer;
        }

        public TextWriter Writer { get; }

        public bool EmitDebugInfo { get; set; }

        public bool EmitDebugBreakpoint { get; set; }

        public bool EmitDebug => this.EmitDebugBreakpoint || this.EmitDebugInfo;

        public void Begin()
        {
            this.WriteRaw("<->");
            this.WriteCommentLine();
        }

        public void End()
        {
            this.Writer.Flush();
        }

        public void Write(Instruction instruction)
        {
            if (this.depthLevel++ == 0 && this.EmitDebug)
            {
                if (this.EmitDebugBreakpoint)
                {
                    this.WriteRaw(Breakpoint);
                }

                this.WriteCommentLine(instruction.ToString());
            }

            var proto = instruction.Prototype;
            if (proto == InsnProtos.ReadChar)
            {
                this.WriteSource(">,");
            }
            else if (proto == InsnProtos.PrintChar)
            {
                this.WriteSource(".");
            }
            else if (proto == InsnProtos.PrintInt)
            {
                this.WriteSource("[>>+>+<<<-]>>>[<<<+>>>-]<<+>[<->[>++++++++++<" +
                    "[->-[>+>>]>[+[-<+>]>+>>]<<<<<]>[-]++++++++[<++++++>-]>" +
                    "[<<+>>-]>[<<+>>-]<<]>]<[->>++++++++[<++++++>-]]<[.[-]<]<");
            }
            else if (proto == InsnProtos.PushInt)
            {
                this.WriteSource(">" + new string('+', instruction.Args[0]));
            }
            else if (proto == InsnProtos.PopInt)
            {
                this.WriteSource("[-]<");
            }
            else if (proto == InsnProtos.DupInt)
            {
                this.WriteSource("[->+>+<<]>>[-<<+>>]<");
            }
            else if (proto == InsnProtos.SwapInt)
            {
                this.WriteSource("[->+<]<[->+<]>>[-<<+>>]<");
            }
            else if (proto == InsnProtos.Not)
            {
                this.WriteSource("[>+<[-]]+>[<->-]<");
            }
            else if (proto == InsnProtos.Add)
            {
                this.WriteSource("[-<+>]<");
            }
            else if (proto == InsnProtos.Sub)
            {
                this.WriteSource("[-<->]<");
            }
            else if (proto == InsnProtos.Mul)
            {
                this.WriteSource("<[>>>+<<<-]>>>[<<[<+>>+<-]>[<+>-]>-]<<");
                this.Write(new Instruction(InsnProtos.PopInt));
            }
            else if (proto == InsnProtos.Div)
            {
                this.WriteSource("[->+<]<[->+>-[>+>>]>[+[-<+>]>+>>]<<<<<<]>>>>[-<<<<+>>>>]<");
                this.Write(new Instruction(InsnProtos.PopInt));
                this.Write(new Instruction(InsnProtos.PopInt));
                this.Write(new Instruction(InsnProtos.PopInt));
            }
            else if (proto == InsnProtos.UpdateScope)
            {
                var types = instruction.Args
                    .Select(arg => DataTypes.GetFromId(arg)).ToList();
                var invalidType = types
                    .FirstOrDefault(tp => tp.MoveLeft == null || tp.MoveRight == null);
                if (invalidType != null)
                {
                    throw new InvalidOperationException($"cannot move around type {invalidType}");
                }

                this.currentVariableTypes = types;
            }
            else if (proto == InsnProtos.WriteLocal)
            {
                int varIndex = instruction.Get(0);
                var heap = this.MoveFromStackToHeap(varIndex);
                var stack = this.MoveFromHeapToStack(varIndex);
                this.WriteRaw($"{heap}[-]{stack}[-{heap}+{stack}]<<");
            }
            else if (proto == InsnProtos.ReadLocal)
            {
                int varIndex = instruction.Get(0);
                var heap = this.MoveFromStackToHeap(varIndex);
                var stack = this.MoveFromHeapToStack(varIndex);
                this.WriteRaw($">>{heap}[-{stack}+{heap}]{stack}");
            }
            else if (proto == InsnProtos.IfElseBegin)
            {
                this.WriteRaw(">+<[>-<");
                this.Write(new Instruction(InsnProtos.PopInt));
            }
            else if (proto == InsnProtos.IfElseElse)
            {
                this.WriteRaw(">]>[-<");
                this.WriteSource("<"); // PopInt (cell is zero)
            }
            else if (proto == InsnProtos.IfElseEnd)
            {
                this.WriteRaw(">>]<<");
            }
            else if (proto == InsnProtos.FsmBegin)
            {
                this.fsmCaseNumber.Push(0);
                this.fsmCaseCount.Push(instruction.Get(0));
                this.Write(new Instruction(InsnProtos.PushInt, new int[] { 1 }));
                this.WriteSource("[");
            }
            else if (proto == InsnProtos.FsmEnd)
            {
                this.WriteSource("-]<");
                var count = this.fsmCaseCount.Peek();
                var number = this.fsmCaseNumber.Peek();
                if (count != number)
                {
                    throw new Exception("illegal case count");
                }

                this.fsmCaseNumber.Pop();
                this.fsmCaseCount.Pop();
            }
            else if (proto == InsnProtos.FsmCase)
            {
                this.fsmCaseNumber.Push(this.fsmCaseNumber.Pop() + 1);
                this.WriteSource("-");
                this.Write(new Instruction(InsnProtos.DupInt, new int[0]));
                this.Write(new Instruction(InsnProtos.Not, new int[0]));
                this.WriteSource("[-<<");
            }
            else if (proto == InsnProtos.FsmJmp)
            {
                int newOffset = InstructionUtil.CalculateFsmCaseOffset(
                        this.fsmCaseNumber.Peek(), instruction.Get(0), this.fsmCaseCount.Peek());
                this.Write(new Instruction(InsnProtos.PushInt, new int[] { newOffset }));
                this.WriteSource(FsmEndCase);
            }
            else if (proto == InsnProtos.FsmIf)
            {
                int offsetTrue = InstructionUtil.CalculateFsmCaseOffset(
                        this.fsmCaseNumber.Peek(), instruction.Get(0), this.fsmCaseCount.Peek());
                int offsetFalse = InstructionUtil.CalculateFsmCaseOffset(
                        this.fsmCaseNumber.Peek(), instruction.Get(1), this.fsmCaseCount.Peek());

                this.Write(new Instruction(InsnProtos.IfElseBegin));
                this.Write(new Instruction(InsnProtos.PushInt, new int[] { offsetTrue }));
                this.Write(new Instruction(InsnProtos.IfElseElse));
                this.Write(new Instruction(InsnProtos.PushInt, new int[] { offsetFalse }));
                this.Write(new Instruction(InsnProtos.IfElseEnd));
                this.WriteSource(FsmEndCase);
            }
            else if (proto == InsnProtos.FsmCall)
            {
                // push next case as return adress
                // TODO check if return adress should be retrieved from args
                this.Write(new Instruction(InsnProtos.PushInt, new int[] { this.fsmCaseNumber.Peek() + 1 }));
                this.Write(new Instruction(InsnProtos.FsmJmp, new int[] { instruction.Get(0) }));

                // Write(new Instruction(InsnProtos.PushInt, new int[] { fsmCaseNumber.Peek() }));
                // writeSrc(">]<");
            }
            else if (proto == InsnProtos.FsmReturn)
            {
                // TODO replace with optimized version AddConstInt[n] or so
                var casesToFsmEnd = this.fsmCaseCount.Peek() - this.fsmCaseNumber.Peek();
                this.Write(new Instruction(InsnProtos.PushInt, new int[] { casesToFsmEnd + 1 }));
                this.Write(new Instruction(InsnProtos.Add));
                this.WriteSource(FsmEndCase);
            }
            else if (proto == InsnProtos.Breakpoint)
            {
                this.WriteRaw(Breakpoint);
            }
            else if (proto == InsnProtos.Nop)
            {
                // nop
            }
            else
            {
                throw new Exception("unsupported instruction: " + instruction.Prototype.Name);
            }

            if (--this.depthLevel == 0)
            {
                this.WriteCommentLine();
            }
        }

        private string MoveFromHeapToStack(int varIndex)
        {
            int heapIndex = this.GetHeapIndex(varIndex);
            return this.MoveRight(heapIndex, 0) + ">->>[->>]<";
        }

        private string MoveFromStackToHeap(int varIndex)
        {
            int heapIndex = this.GetHeapIndex(varIndex);
            return "<+[<<+]<" + this.MoveLeft(0, heapIndex);
        }

        private string MoveRight(int startVarIndex, int endVarIndex)
        {
            Debug.Assert(startVarIndex >= endVarIndex, "startIndex must not be smaller than endIndex");
            if (this.currentVariableTypes == null)
            {
                throw new InvalidOperationException("there is no scope layout specified");
            }

            StringBuilder result = new StringBuilder();
            for (int i = startVarIndex - 1; i >= endVarIndex; i--)
            {
                result.Append(this.currentVariableTypes[i].MoveRight);
            }

            return result.ToString();
        }

        private string MoveLeft(int startVarIndex, int endVarIndex)
        {
            Debug.Assert(startVarIndex <= endVarIndex, "startIndex must not be greater than endIndex");
            if (this.currentVariableTypes == null)
            {
                throw new InvalidOperationException("there is no scope layout specified");
            }

            StringBuilder result = new StringBuilder();
            for (int i = startVarIndex; i < endVarIndex; i++)
            {
                result.Append(this.currentVariableTypes[i].MoveLeft);
            }

            return result.ToString();
        }

        private int GetHeapIndex(int varIndex)
        {
            // if (context.VariableForReturnAddress)
            int heapIndex = varIndex /* + 1 */;
            if (varIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(varIndex));
            }

            return heapIndex;
        }

        private void WriteSource(string srcString)
        {
            srcString = srcString.Replace(">", ">>").Replace("<", "<<");
            this.WriteRaw(srcString);
        }

        private void WriteRaw(string rawString)
        {
            this.Writer.Write(rawString);
        }

        private void WriteCommentLine(string commentString = "")
        {
            if (this.EmitDebugInfo)
            {
                commentString = commentString
                    .Replace('<', '{').Replace('>', '}')
                    .Replace('-', '_').Replace('+', '_')
                    .Replace('[', '(').Replace(']', ')')
                    .Replace('.', ':').Replace(',', ';')
                    .Replace(Breakpoint, "_");
                this.WriteRaw(commentString);
                this.WriteRaw("\n");
            }
        }
    }
}
