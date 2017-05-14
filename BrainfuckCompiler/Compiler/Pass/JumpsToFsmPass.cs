using System;
using System.Collections.Generic;
using System.Linq;
using BrainfuckCompiler.Compiler.CodeGenerator;
using static BrainfuckCompiler.Compiler.CodeGenerator.InstructionPrototypes;

namespace BrainfuckCompiler.Compiler.Pass
{
    /// <summary>
    /// Replaces jump and label instructions with a FSM
    /// </summary>
    public class JumpsToFsmPass : ICompilerPass
    {
        public List<Instruction> Pass(List<Instruction> list)
        {
            var listsOfInstructions = this.SplitInstructions(list);
            if (listsOfInstructions.Count == 0 ||
               (listsOfInstructions.Count == 1 && !IsJump(listsOfInstructions[0].Last().Prototype)))
            {
                return list;
            }

            return this.JoinInstructions(listsOfInstructions);
        }

        /// <summary>Splits the given instruction list before any jump and/or after any label. Empty lists are discarded.</summary>
        private List<List<Instruction>> SplitInstructions(List<Instruction> list)
        {
            var result = new List<List<Instruction>>();
            var currentList = new List<Instruction>();
            Instruction lastInstruction = null;
            foreach (var instruction in list)
            {
                var insnWasJump = IsJump(lastInstruction?.Prototype);
                var insnIsLabel = IsLabel(instruction.Prototype);
                if ((insnWasJump || insnIsLabel) && currentList.Count != 0)
                {
                    result.Add(currentList);
                    currentList = new List<Instruction>();
                }

                currentList.Add(instruction);
                lastInstruction = instruction;
            }

            if (currentList.Count != 0)
            {
                result.Add(currentList);
            }

            return result;
        }

        /// <summary>Joins multiple instruction lists together replacing jump and label instructions with a FSM</summary>
        /// <param name="listsOfInstructions">Multiple Lists of instructions. Every list must either start with a label or end with a jump</param>
        /// <returns>List of Instructions</returns>
        private List<Instruction> JoinInstructions(List<List<Instruction>> listsOfInstructions)
        {
            var labelToIndex = new Dictionary<int, int>();
            for (int i = 0; i < listsOfInstructions.Count; i++)
            {
                var first = listsOfInstructions[i].First();
                if (IsLabel(first.Prototype))
                {
                    labelToIndex[first.Get(0)] = i + 1;
                }
            }

            List<Instruction> result = new List<Instruction>();
            result.Add(FsmBegin, new int[] { listsOfInstructions.Count });
            for (int i = 0; i < listsOfInstructions.Count; i++)
            {
                var first = listsOfInstructions[i].First();
                var last = listsOfInstructions[i].Last();
                int start = 0, end = listsOfInstructions[i].Count;
                if (IsLabel(first.Prototype))
                {
                    start++;
                }

                if (IsJump(last.Prototype))
                {
                    end--;
                }

                result.Add(FsmCase);
                result.AddRange(listsOfInstructions[i].SubList(start, end - start));
                AddFsmJump(labelToIndex, result, i + 1, last);
            }

            result.Add(FsmEnd);
            return result;
        }

        private static void AddFsmJump(Dictionary<int, int> labelToIndex, List<Instruction> result, int currentFsmId, Instruction last)
        {
            if (last.Prototype == Return)
            {
                result.Add(FsmReturn);
            }
            else if (IsJump(last.Prototype))
            {
                var label = last.Get(0);
                var fsmId = labelToIndex[label];
                if (last.Prototype == Jump)
                {
                    result.Add(FsmJmp, new int[] { fsmId });
                }
                else if (last.Prototype == JumpIf)
                {
                    int trueJump = fsmId, falseJump = currentFsmId + 1;

                    // micro optimisation: not[], jumpIf[a,b] -> jumpIf[b,a]
                    if (result.Last().Prototype == Not)
                    {
                        result.RemoveAt(result.Count - 1);
                        Swap(ref trueJump, ref falseJump);
                    }

                    result.Add(FsmIf, new int[] { trueJump, falseJump });
                }
                else if (last.Prototype == Invoke)
                {
                    result.Add(FsmCall, new int[] { fsmId });
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                // fall through: just jump to the next fsm state
                result.Add(FsmJmp, new int[] { currentFsmId + 1 });
            }
        }

        private static bool IsJump(InstructionPrototype proto)
            => proto == Jump || proto == JumpIf || proto == Invoke || proto == Return;

        private static bool IsLabel(InstructionPrototype proto)
            => proto == Label;

        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            var tmp = lhs;
            lhs = rhs;
            rhs = tmp;
        }
    }
}
